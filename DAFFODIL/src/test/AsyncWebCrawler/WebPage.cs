using System;
using System.Collections.Generic;
using AsyncWebCrawler.Interfaces;
using AsyncWebCrawler.Repositories;
using AsyncWebCrawler.Logging;
using System.Threading.Tasks;

namespace AsyncWebCrawler
{
    public class WebPage
    {
        #region Private Fields

        /// <summary>
        /// external URL repository
        /// </summary>
        private IRepository _externalUrlRepository;

        /// <summary>
        /// Other URL repository
        /// </summary>
        private IRepository _otherUrlRepository;

        /// <summary>
        /// Failed URL repository
        /// </summary>
        private IRepository _failedUrlRepository;

        /// <summary>
        /// Current page URL repository
        /// </summary>
        private IRepository _currentPageUrlRepository; 

        /// <summary>
        /// List of Pages.
        /// </summary>
        private static List<Page> _pages = new List<Page>();

        /// <summary>
        /// List of exceptions.
        /// </summary>
        private static List<string> _exceptions = new List<string>();          

        /// <summary>
        /// Is current page or not
        /// </summary>
        private bool isCurrentPage = true;

        #endregion

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public void Initialize()
        {
            _externalUrlRepository = new ExternalUrlRepository();

            _otherUrlRepository = new OtherUrlRepository();

            _failedUrlRepository = new FailedUrlRepository();

            _currentPageUrlRepository = new CurrentPageUrlRepository(); 
        }

        /// <summary>
        /// Creating the report.
        /// </summary>
        public void CreateReport(string logFileName)
        {
            try
            {
                Logging.Logging.LogFile = logFileName;
                var stringBuilder = Reporting.CreateReport(_externalUrlRepository, _otherUrlRepository, _failedUrlRepository, _currentPageUrlRepository, _pages, _exceptions);
                Logging.Logging.WriteReportToDisk(stringBuilder.ToString());
            }
            catch (Exception e)
            {
                if (e is System.IO.IOException && (e.HResult & 0x0000FFFF) != 5664)
                {
                    Console.WriteLine("Aborting report creation: {0}", e.Message);
                    return;
                }
                throw e;
            }
        }

        /// <summary>
        /// Crawls a page.
        /// </summary>
        /// <param name="url">The url to crawl.</param>
        public Task<int> ProcessPage(string url, string htmlText, CrawlList crawlList)
        {
            Task<int> processTask;
            processTask = Task<int>.Run(() =>
            {
                var linkParser = new LinkParser();
                var page = new Page();
                page.Text = htmlText;
                page.Url = url;
                _pages.Add(page);
                linkParser.ParseLinks(page, url);

                foreach (string exception in linkParser.Exceptions)
                    _exceptions.Add(exception);

                isCurrentPage = false;
                //Crawl all the links found on the page.
                foreach (string link in linkParser.ExternalUrls)
                {
                    if (_externalUrlRepository.List.Contains(link)) continue;
                    string formattedLink = link;
                    try
                    {
                        formattedLink = FixPath(url, formattedLink);

                        if (formattedLink != String.Empty)
                        {
                            crawlList.AddUrl(formattedLink);
                        }
                    }
                    catch (Exception exc)
                    {
                        _failedUrlRepository.List.Add(formattedLink + " (on page at url " + url + ") - " + exc.Message);
                    }
                }

                //Add data to main data lists
                if (isCurrentPage)
                {
                    AddRangeButNoDuplicates(_currentPageUrlRepository.List, linkParser.ExternalUrls);
                }

                AddRangeButNoDuplicates(_externalUrlRepository.List, linkParser.ExternalUrls);
                AddRangeButNoDuplicates(_otherUrlRepository.List, linkParser.OtherUrls);
                AddRangeButNoDuplicates(_failedUrlRepository.List, linkParser.BadUrls);
                return 0;
            });
            return processTask;
        }

        /// <summary>
        /// Fixes a path. Makes sure it is a fully functional absolute url.
        /// </summary>
        /// <param name="originatingUrl">The url that the link was found in.</param>
        /// <param name="link">The link to be fixed up.</param>
        /// <returns>A fixed url that is fit to be fetched.</returns>
        public static string FixPath(string originatingUrl, string link)
        {
            string formattedLink = String.Empty;

            if (link.IndexOf("../") > -1)
            {
                formattedLink = ResolveRelativePaths(link, originatingUrl);
            }
            else
            {
                formattedLink = link;
            }
            return formattedLink;
        }

        /// <summary>
        /// Needed a method to turn a relative path into an absolute path. And this seems to work.
        /// </summary>
        /// <param name="relativeUrl">The relative url.</param>
        /// <param name="originatingUrl">The url that contained the relative url.</param>
        /// <returns>A url that was relative but is now absolute.</returns>
        public static string ResolveRelativePaths(string relativeUrl, string originatingUrl)
        {
            if (relativeUrl == "" || originatingUrl == "") throw new ArgumentNullException();
            string resolvedUrl = String.Empty;
            string[] relativeUrlArray = relativeUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] originatingUrlElements = originatingUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (relativeUrlArray.Length == 0 || originatingUrlElements.Length == 0) throw new FormatException();
            int indexOfFirstNonRelativePathElement = 0;
            for (int i = 0; i <= relativeUrlArray.Length - 1; i++)
            {
                if (relativeUrlArray[i] != "..")
                {
                    indexOfFirstNonRelativePathElement = i;
                    break;
                }
            }

            int countOfOriginatingUrlElementsToUse = originatingUrlElements.Length - indexOfFirstNonRelativePathElement - 1;
            for (int i = 0; i <= countOfOriginatingUrlElementsToUse - 1; i++)
            {
                if (originatingUrlElements[i] == "http:" || originatingUrlElements[i] == "https:")
                    resolvedUrl += originatingUrlElements[i] + "//";
                else
                    resolvedUrl += originatingUrlElements[i] + "/";
            }

            for (int i = 0; i <= relativeUrlArray.Length - 1; i++)
            {
                if (i >= indexOfFirstNonRelativePathElement)
                {
                    resolvedUrl += relativeUrlArray[i];

                    if (i < relativeUrlArray.Length - 1)
                        resolvedUrl += "/";
                }
            }

            return resolvedUrl;
        }

        /// <summary>
        /// Merges a two lists of strings.
        /// </summary>
        /// <param name="targetList">The list into which to merge.</param>
        /// <param name="sourceList">The list whose values need to be merged.</param>
        private static void AddRangeButNoDuplicates(List<string> targetList, List<string> sourceList)
        {
            foreach (string str in sourceList)
            {
                if (!targetList.Contains(str))
                    targetList.Add(str);
            }
        }
    }
}
