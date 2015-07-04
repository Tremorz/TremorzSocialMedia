using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Globalization;
using System.Data;

namespace GetTweets
{
    class TwitterReader
    {
        // twitter connections
        private string bearerToken;
        private string oauth_consumer_key = "7cP6qSSZ10GdMUs8LEjV7x8l6";
        private string oauth_consumer_secret = "UlOEi5n74KPsKM8CN84yqpjZc3WC1eATHgaZuBPYJ6uWY6M78l";

        // sql connections
        private string db_connection_string = "Data Source=txpxz2472h.database.windows.net;Initial Catalog=TremorzGovHack2015;Database=TremorzGovHack2015;User Id=TremorzAdmin;Password=Tremorz@2015";

        // twitter interactions
        private string tokenApi = "https://api.twitter.com/oauth2/token";
        private string searchApi = "https://api.twitter.com/1.1/search/tweets.json";

        // things we're watching
        // mapping from searchable item to largest id of the last search we ran against that so we can avoid duplicate data (note, still getting duplicates, not sure why.)
        private Dictionary<string, string> hashtagsDict = new Dictionary<string, string>();     
        private Dictionary<string, string> usersDict = new Dictionary<string, string>();

        // db interactions
        private string updateDBQuery = @"INSERT INTO datadump.Tweets 
	                                        (Username, TweetDate, TweetText, SearchHashtag, Hashtags, RetweetCount, InsertRun)
                                        VALUES ";

        public TwitterReader()
        {
            // load what we want to watch (hashtags and twitter users)
            hashtagsDict.Add("#Weather", "0");
            usersDict.Add("@metlinkwgtn", "0");
            usersDict.Add("@tranzmetro", "0");
            usersDict.Add("@nzcivildefence", "0");
            usersDict.Add("@nztawgtn", "0");

            // login and get auth token
            SetBearerToken();
        }


        // run searches for everything we're watching and load to database
        public void Run(int runInstance)
        {
            // hashtag searches
            Dictionary<string, string> updatedHashtagsDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> hashtag in hashtagsDict)
            {
                TweetsList data = RunSearch(hashtag.Key, hashtag.Value);
                WriteToDatabase(hashtag.Key, data, runInstance);
                if (data.Statuses.Count > 0)
                    updatedHashtagsDict.Add(hashtag.Key, data.Statuses.Last().Id);
            }
            hashtagsDict = updatedHashtagsDict;


            // user searches
            Dictionary<string, string> updatedUsersDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> user in usersDict)
            {
                TweetsList data = RunSearch(user.Key, user.Value);
                WriteToDatabase("", data, runInstance);
                if (data.Statuses.Count > 0)
                    updatedUsersDict.Add(user.Key, data.Statuses.Last().Id);
            }
            usersDict = updatedUsersDict;
        }


        private void SetBearerToken()
        {
            //Token URL
            var headerFormat = "Basic {0}";
            var authHeader = string.Format(headerFormat,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oauth_consumer_key) + ":" +
            Uri.EscapeDataString((oauth_consumer_secret)))
            ));

            var postBody = "grant_type=client_credentials";

            ServicePointManager.Expect100Continue = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tokenApi);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            request.Headers.Add("Accept-Encoding", "gzip");
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
            using (var reader = new StreamReader(responseStream))
            {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
                bearerToken = data["access_token"];
            }
        }


        private TweetsList RunSearch(string query, string since_id)
        {
            // build search string. Note, since_id=0 means first search we've done
            string postBody = "q=";
            if (since_id.Equals("0"))
                postBody += Uri.EscapeDataString(query);
            else
                postBody += Uri.EscapeDataString(query) + "&since_id=" + since_id;

            var resource_url = searchApi + "?" + postBody;

            // build request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", "Bearer " + bearerToken);
            request.Method = "GET";
            request.ContentType = "*/*";

            // read request and convert to usable objects
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();
            TweetsList data = JsonConvert.DeserializeObject<TweetsList>(json);
            return data;
        }


        private void WriteToDatabase(string searchHashtag, TweetsList tweets, int insertRun) {

            // write any tweets in tweets to the database
            if (tweets.Statuses.Count == 0)
                return;

            StringBuilder updateQuery = new StringBuilder(updateDBQuery);
            foreach (Tweet tweet in tweets.Statuses)
            {
                updateQuery.Append("(");
                updateQuery.Append("'" + tweet.User.Screen_Name + "'");
                updateQuery.Append(",'" + DateTime.ParseExact(tweet.Created_At, "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd hh:mm:ss") + "'");
                updateQuery.Append(",'" + tweet.Text.Replace("'", "''") + "'");
                updateQuery.Append(",'" + searchHashtag + "'");
                updateQuery.Append(",'" + string.Join(",", tweet.Entities.Hashtags.Select(h => h.Text).ToArray()) + "'");
                updateQuery.Append(",'" + tweet.Retweet_Count + "'");
                updateQuery.Append("," + insertRun.ToString());
                updateQuery.Append("),");
            }
            string dbQuery = updateQuery.Remove(updateQuery.Length-1, 1).ToString();

            SqlConnection con = new SqlConnection(db_connection_string);
            SqlCommand cmd = new SqlCommand(dbQuery, con);

            cmd.CommandType = CommandType.Text;

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
