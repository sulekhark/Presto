using System.Collections.Generic;

namespace AsyncWebCrawler.Interfaces
{
    /// <summary>
    /// Repository interface
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// List of Url
        /// </summary>
        List<string> List { get; }

        /// <summary>
        /// To add url into List.
        /// </summary>
        /// <param name="url"></param>
        void Add(string url);                
    }
}
