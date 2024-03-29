﻿using System.Collections.Generic;
using AsyncWebCrawler.Interfaces;

namespace AsyncWebCrawler.Repositories
{
    public class CurrentPageUrlRepository : IRepository
    {
        /// <summary>
        /// List of external Urls.
        /// </summary>
        List<string> _listOfCurrentPageUrl;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public CurrentPageUrlRepository()
        {
            _listOfCurrentPageUrl = new List<string>();
        }

        /// <summary>
        /// List to gather Urls.
        /// </summary>
        public List<string> List
        {
            get
            {
                return _listOfCurrentPageUrl;
            }
        }

        /// <summary>
        /// Method to add new Url.
        /// </summary>
        /// <param name="entity"></param>
        public void Add(string entity)
        {
            _listOfCurrentPageUrl.Add(entity);
        }
    }
}
