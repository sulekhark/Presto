using System.Collections.Generic;
using AsyncWebCrawler.Interfaces;

namespace AsyncWebCrawler.Repositories
{
    /// <summary>
    /// Class for External Urls.
    /// </summary>
    public class OtherUrlRepository : IRepository
    {
        /// <summary>
        /// List of external Urls.
        /// </summary>
        List<string> _listOfOtherUrl;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public OtherUrlRepository()
        {
            _listOfOtherUrl = new List<string>();
        }

        /// <summary>
        /// List to gather Urls.
        /// </summary>
        public List<string> List
        {
            get
            {
                return _listOfOtherUrl;
            }
        }

        /// <summary>
        /// Method to add new Url.
        /// </summary>
        /// <param name="entity"></param>
        public void Add(string entity)
        {
            _listOfOtherUrl.Add(entity);
        }
    }
}
