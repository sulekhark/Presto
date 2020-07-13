using System;

namespace AsyncWebCrawler
{
    public class Program
    {
	public static void Main(string[] args)
        {
            var crawler = new Crawler(5);

            try
            {
                UTest();
                crawler.CrawlRoot("https://www.seas.upenn.edu/~sulekha/AsyncWebCrawlerTest/srk_test.html");
                crawler.CreateReport("crawl_log.html");
            }
            catch (Exception e)
            {
                Exception ex;
                if (e is AggregateException)
                {
                    ex = e.InnerException;
                }
                else
                {
                    ex = e;
                }
                Console.WriteLine("Got exception: {0}", ex.Message);
                if (!Crawler.LogOnly(ex)) throw ex;
            }
        }

        public static void UTest()
        {
            try
            {
                UnitTest.FixPathMethodTest();
                UnitTest.ResolveRelativePathsMethodTest();
            }
            catch (Exception e)
            {
                Console.WriteLine("Got exception while unit testing: {0}", e.Message);
            }
        }
    }
}
