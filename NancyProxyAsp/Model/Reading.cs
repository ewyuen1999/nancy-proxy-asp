using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyProxyAsp.Model
{
    public class Reading
    {
        public string Book { get; set; }

        public int BeginChapter { get; set; }

        public int BeginChapterStartVerse { get; set; }

        public int BeginChapterEndVerse { get; set; }

        public int EndChapter { get; set; }

        public int EndChapterStartVerse { get; set; }

        public int EndChapterEndVerse { get; set; }

    }
}