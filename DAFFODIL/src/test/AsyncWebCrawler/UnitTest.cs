
namespace AsyncWebCrawler 
{
    public class UnitTest
    {
        public static void FixPathMethodTest()
        {
            var actualResult = WebPage.FixPath("http://python.org", @"/static/stylesheets/mq.css");
        }

        public static void ResolveRelativePathsMethodTest()
        {
            var actualResult = WebPage.ResolveRelativePaths(@"../styles/ChannelMod.css", "http://web.mit.edu/aboutmit");
        }
    }
}
