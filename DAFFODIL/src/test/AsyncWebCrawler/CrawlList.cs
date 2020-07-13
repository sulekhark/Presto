using System.Collections.Generic;

public class CrawlList
        {
            public readonly Queue<string> UrlsToCrawl;
            public readonly List<string> UrlsCompleted;

            public CrawlList()
            {
                UrlsToCrawl = new Queue<string>();
                UrlsCompleted = new List<string>();
            }

            public bool HasNext()
            {
                return UrlsToCrawl.Count > 0;
            }

            public string GetNext()
            {
                return UrlsToCrawl.Dequeue();
            }

            public void AddUrls(List<string> urls)
            {
                foreach (var url in urls)
                {
                    AddUrl(url);
                }
            }
            public void AddUrl(string url)
            {
                if (UrlAlreadyAdded(url)) return;

                UrlsToCrawl.Enqueue(url);
            }

            public bool UrlAlreadyAdded(string url)
            {
                return UrlsToCrawl.Contains(url) || UrlsCompleted.Contains(url);
            }
        }
