using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AsyncWebCrawler
{
    public class Crawler
    {
        public readonly CrawlList CrawlList;
        private readonly Queue<Task<Tuple<string, string>>> runningTasks = new Queue<Task<Tuple<string, string>>>();
        private readonly int maxConcurrentDownload;
        private readonly WebPage webPage;

        public Crawler(int maxConcurrentDownload)
        {
            CrawlList = new CrawlList();
            this.maxConcurrentDownload = maxConcurrentDownload;
            ServicePointManager.DefaultConnectionLimit = maxConcurrentDownload;
            webPage = new WebPage();
            webPage.Initialize();
        }

        public void CrawlRoot(string url)
        {
            Crawl(url).Wait();
        }

        public void CreateReport(string logFileName)
        {
            webPage.CreateReport(logFileName);
        }

        public async Task<bool> Crawl(string startUrl)
        {
            runningTasks.Enqueue(ProcessUrl(startUrl));
            while (runningTasks.Count > 0)
            {
                try
                {
                    var nextElem = runningTasks.Dequeue();
                    var urlResult = await nextElem;
                    string pageText = urlResult.Item1;
                    Console.WriteLine("Webpage contents: {0}", pageText);
                    int done = await webPage.ProcessPage(urlResult.Item2, pageText, CrawlList);
                }
                catch (Exception e)
                {
                    Exception wrapEx = new Exception("Error in method Crawl.", e);
                    throw wrapEx;
                }

                while (CrawlList.HasNext() && runningTasks.Count < maxConcurrentDownload)
                {
                    var url = CrawlList.GetNext();
                    runningTasks.Enqueue(ProcessUrl(url));
                }
            }
            return true;
        }

        private async Task<Tuple<string, string>> ProcessUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.UserAgent = "A Web Crawler";
            Stream stream = await GetResponseStream(request);
            string htmlText = await ReadFromStream(stream);
            return new Tuple<string, string>(htmlText, url);
        }

        private Task<Stream> GetResponseStream(HttpWebRequest req)
        {
            Task<Stream> wTask;
            wTask = Task<Stream>.Run(() =>
            {
                if (req == null) throw new ArgumentNullException();
                WebResponse response = req.GetResponse();
                Stream wStream = response.GetResponseStream();
                if (wStream == null) throw new InvalidOperationException();
                return wStream;
            });
            return wTask;
        }

        private Task<String> ReadFromStream(Stream stream)
        {
            Task<String> rdTask;
            rdTask = Task<String>.Run(() =>
            {
                StreamReader reader = new StreamReader(stream);
                string htmlText = reader.ReadToEnd();
                if (htmlText == "") throw new FormatException();
                return htmlText;
            });
            return rdTask;
        }

        public static bool LogOnly(Exception e)
        {
            if (e is ArgumentException ||
                e is ArgumentOutOfRangeException ||
                e is ArgumentNullException)
                return true;
            return false;
        }
    }
}
