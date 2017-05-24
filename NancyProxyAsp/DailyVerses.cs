using Nancy.Responses;
using Newtonsoft.Json;

namespace NancyProxyAsp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using Nancy;

    using Newtonsoft.Json.Linq;
    using Model;

    public class DailyVerses : NancyModule
    {
        private static JObject _schedules;

        static DailyVerses()
        {
            var codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            var uri = new Uri(codeBase);
            var path = Path.Combine(uri.AbsolutePath, "data", "schedule.json");
            _schedules = JObject.Parse(File.ReadAllText(path));
        }
        
        public DailyVerses()
        {
            Get["/schedule/"] = _ =>
                {
                    var today = DateTime.Today;
                    return  Response.AsJson(GetSchedule(today.Month, today.Day));
                };

            Get["/schedule/{mm}/{dd}"] = parameters =>
                {
                    var month = int.Parse(parameters.mm);
                    var day = int.Parse(parameters.dd);
                    var json = JsonConvert.SerializeObject(GetSchedule(month, day));
                    return json;
                };
        }

        private string GetSchedule2(int month, int day)
        {
            var key = string.Format("{0:D2}{1:D2}", month, day);
            return (string)_schedules[key];
        }

        /// <summary>
        /// The get schedule.
        /// </summary>
        /// <param name="month">
        /// The month.
        /// </param>
        /// <param name="day">
        /// The day.
        /// </param>
        /// <returns>
        /// The <see cref="Schedule"/>.
        /// </returns>
        private Schedule GetSchedule(int month, int day)
        {
            var key = string.Format("{0:D2}{1:D2}", month, day);
            var verses = (string)_schedules[key];

            var schedule = new Schedule();
            schedule.Date = key;
            schedule.Verses = verses;
            schedule.Devotions = verses.Split(';');
            foreach (var devotion in schedule.Devotions)
            {
                var reading = DevotionToReading(devotion);
                var chapters = ReadingToChapters(reading);
                foreach (var chapter in chapters)
                {
                    schedule.Chapters.Add(chapter);
                }
            }
            return schedule;
        }

        private IList<EachChapter> ReadingToChapters(Reading reading)
        {
            var chapters = new List<EachChapter>();
            for (int i = reading.BeginChapter; i <= reading.EndChapter; i++)
            {
                var chapter = new EachChapter
                {
                    Book = reading.Book,
                    Chapter = i,
                };

                if (i == reading.BeginChapter)
                {
                    chapter.StartVerse = reading.BeginChapterStartVerse;
                    chapter.EndVerse = reading.EndChapterEndVerse;
                }
                else
                if (i == reading.EndChapter)
                {
                    chapter.StartVerse = reading.EndChapterStartVerse;
                    chapter.EndVerse = reading.EndChapterEndVerse;
                }
                else
                {
                    chapter.StartVerse = 1;
                    chapter.EndVerse = 1000;
                }

                chapters.Add(chapter);
            }

            return chapters;
        }

        private Reading DevotionToReading(string devotion)
        {
            var spaceIndex = devotion.IndexOf(" ");
            var colonIndex = devotion.IndexOf(":");
            var secondColonIndex = devotion.LastIndexOf(":");
            if (secondColonIndex == colonIndex)
            {
                secondColonIndex = -1;
            }

            var dashIndex = devotion.IndexOf("-");
            var secondDashIndex = devotion.LastIndexOf("-");
            if (dashIndex == secondDashIndex)
            {
                secondDashIndex = -1;
            }


            var entry = new Reading();
            entry.Book = devotion.Substring(0, spaceIndex);
            entry.BeginChapter = Int32.Parse(devotion.Substring(spaceIndex, colonIndex - spaceIndex));
            if (entry.BeginChapter == 0)
            {
                entry.BeginChapter = 1;
            }

            // get chapters
            if (dashIndex == -1)
            {
                entry.EndChapter = entry.BeginChapter;
            }
            else
            {
                if (secondColonIndex != -1)
                {
                    entry.EndChapter = Int32.Parse(devotion.Substring(dashIndex + 1, secondColonIndex - dashIndex - 1));
                }
                else
                {
                    entry.EndChapter = entry.BeginChapter;
                }
            }
            if (entry.EndChapter == 0)
            {
                entry.EndChapter = 1;
            }

            // get verses
            if (dashIndex == -1)
            {
                entry.BeginChapterStartVerse = 1;
                entry.BeginChapterEndVerse = 1000;
                entry.EndChapterStartVerse = entry.BeginChapterStartVerse;
                entry.EndChapterEndVerse = entry.BeginChapterEndVerse;
            }
            else
            {
                if (secondDashIndex == -1)
                {
                    if (entry.BeginChapter == entry.EndChapter)
                    {
                        // e.g. Psalm 145:8-13
                        entry.BeginChapterStartVerse = Int32.Parse(devotion.Substring(colonIndex + 1, dashIndex - colonIndex - 1));
                        entry.BeginChapterEndVerse = Int32.Parse(devotion.Substring(dashIndex + 1, devotion.Length - dashIndex - 1));
                        entry.EndChapterStartVerse = entry.BeginChapterStartVerse;
                        entry.EndChapterEndVerse = entry.BeginChapterEndVerse;
                    }
                    else
                    {
                        entry.BeginChapterStartVerse = Int32.Parse(devotion.Substring(colonIndex + 1, dashIndex - colonIndex - 1));
                        entry.BeginChapterEndVerse = 1000;
                        entry.EndChapterStartVerse = 1;
                        if (secondDashIndex == -1)
                        {
                            if (secondColonIndex > 0)
                            {
                                // e.g. Zechariah 4:1-5:11
                                entry.EndChapterEndVerse = Int32.Parse(devotion.Substring(secondColonIndex + 1, devotion.Length - secondColonIndex - 1));
                            }
                        }
                    }
                }
            }

            return entry;
        }
    }
}