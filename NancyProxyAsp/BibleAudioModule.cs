using HtmlAgilityPack;
using Nancy;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace NancyProxyAsp
{
    public class BibleAudioModule : NancyModule
    {
        public BibleAudioModule()
        {
            Get["/bible/{id}/{book}/{chapter}"] = parameters =>
            {
                var url = $"https://www.bible.com/bible/{parameters.id}/{parameters.book}.{parameters.chapter}";
                Console.WriteLine("Getting: {0}", url);
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        var responseString = reader.ReadToEnd();
                        response.Dispose();
                        return GetAudioUrl(responseString);
                    }
                    response.Dispose();
                    return null;
                }

            };

        }

        private string GetAudioUrl(string html)
        {
            var url = html;
            var source = WebUtility.HtmlDecode(html);
            var document = new HtmlDocument();
            document.LoadHtml(source);
            var element = document.DocumentNode.SelectSingleNode("//source");
            if (element != null)
            {
                //var xmlDocument = new XmlDocument();
                ////xmlDocument.LoadXml(element.OuterHtml);
                //var audioElements = xmlDocument.GetElementsByTagName("audio");
                //if (audioElements.Count > 0)
                //{
                url = element.Attributes?["src"].Value;
                Console.WriteLine("Returned: {0}", url);
                return url;
                //}
            }
            return null;
        }
    }
}