using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyProxyAsp.Model
{
    public class Schedule
    {
        public string Date { get; set; }
        public string Verses { get; set; }

        public string[] Devotions { get; set; }

        public IList<EachChapter> Chapters { get; }

        public Schedule()
        {
            Chapters = new List<EachChapter>();
        }

    }
}