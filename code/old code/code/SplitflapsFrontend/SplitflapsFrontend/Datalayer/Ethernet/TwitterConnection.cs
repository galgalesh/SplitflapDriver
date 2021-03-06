﻿using System.Net;
using System.IO;
using System.Collections;
using SplitflapsFrontend.Overkoepelend;
using System.Collections.Generic;

namespace TwitterSplitflaps.Datalayer.Ethernet
{
    static class TwitterConnection
    {

        #region Members

        private static string searchURL = "http://search.twitter.com/search.json?q=";
        private static string fullURL;
        private static string query;

        #endregion

        #region Properties

        public static string Query
        {
            get
            {
                return query;
            }
            set
            {
                // Query starts with # - replace it with %21
                if (value.StartsWith("#")) value = "%21" + value.Substring(0, 1);

                // All else cases
                else if (!value.StartsWith("%21"))
                {
                    value = "%21" + value;
                }

                query = value;
                fullURL = string.Concat(searchURL, query);
            }
        }

        #endregion

        /// <summary>
        /// Gets the latest tweets since a specified ID
        /// </summary>
        /// <param name="sinceID">ID which represents a Tweet ID</param>a
        /// <returns>List containg tweets since a specified ID</returns>
        public static List<Tweet> GetLatestTweetsSince(string sinceID)
        {
            // Perform the WebRequest and parse it
            return ParseResponse(PerformGetRequest(fullURL + "&since_id=" + sinceID));
        }

        /// <summary>
        /// Gets the latest tweets, count specified by param count
        /// </summary>
        /// <param name="count">Howmany tweets that need to be returned</param>
        /// <returns>List containing n Tweet object(s)</returns>
        public static List<Tweet> GetLatestTweets(int count)
        {
            // Perform the WebRequest and parse it
            return ParseResponse(PerformGetRequest(fullURL + "&rpp=" + count));
        }

        /// <summary>
        /// Gets the latest tweet.
        /// </summary>
        /// <returns>Tweet Object</returns>
        public static Tweet GetLatestTweet()
        {
            // Perform the WebRequest
            string html = PerformGetRequest(fullURL + "&rpp=1");
            // Parse it
            List<Tweet> tweets = ParseResponse(html);
            // Check if we have any result
            if (tweets == null) return null;
            // Return result
            return tweets[0];
        }

        /// <summary>
        /// Parses the HTML Response into Tweet objects
        /// </summary>
        /// <param name="html">The HTML which needs to be parsed.</param>
        /// <returns>List containing Tweet object(s)</returns>
        private static List<Tweet> ParseResponse(string html)
        {

            // Check if no tweet was fetched
            if (html == null) return null;

            // Get the index of the first result
            int resultIndex = html.IndexOf("\"results\":[{\"created_at\":");

            // Check if there is no result (no result(s) shows "results:[]" and thus index == -1)
            if (resultIndex == -1) return null;

            // Take apart the results from the full json
            html = html.Substring(resultIndex + 11);

            // ArrayList to store individual results
            ArrayList results = new ArrayList();
            // Bool to indicate wheter there are more results left in the json
            bool moreResults = true;

            // Split the individual results
            do
            {
                resultIndex = html.IndexOf("},{\"created_at\":");
                if (resultIndex == -1)
                {
                    moreResults = false;
                    results.Add(html);
                }
                else
                {
                    results.Add(html.Substring(0, resultIndex));
                    html = html.Substring(resultIndex + 2);
                }
            } while (moreResults);



            // Save the tweets in an ArrayList
            List<Tweet> tweets = new List<Tweet>();

            // Make new tweet object of the results and put them in the ArrayList
            foreach (string result in results)
            {

                // User
                int userIndex = result.IndexOf("\"from_user\":\"") + 13;
                int userEndIndex = result.IndexOf(",\"from_user_id\":");
                string user = result.Substring(userIndex, userEndIndex - userIndex - 1);

                // Text
                int textIndex = result.IndexOf("\"text\":\"") + 8;
                int textEndIndex = result.IndexOf(",\"to_user\":");
                string text = result.Substring(textIndex, textEndIndex - textIndex - 1);

                // ID
                int idIndex = result.IndexOf("\"id\":") + 5;
                int idEndIndex = result.IndexOf(",\"id_str\":");
                string id = result.Substring(idIndex, idEndIndex - idIndex - 1);

                tweets.Add(new Tweet()
                {
                    Text = text,
                    User = user,
                    ID = id
                });

            }

            // Return results
            return tweets;
        }

        /// <summary>
        /// Performs a HTTP GET WebRequest.
        /// </summary>
        /// <param name="url">The URL which needs to be requested.</param>
        /// <returns>The WebResponse of the request.</returns>
        private static string PerformGetRequest(string url)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.KeepAlive = false;
            string responseString = null;

            try
            {
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                responseString = reader.ReadToEnd();
                reader.Close();
            }
            catch
            {
            }

            return responseString;
        }
    }
}
