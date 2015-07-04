using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GetTweets
{
    class FacebookReader
    {
        public FacebookClient client;

        public string accessToken =
            "CAAKCW4KuP4QBAO1mCa2MhNez1HLQ3cUYHIsUY8qaqiSEXioiiRdyuQHlQZAZAZC8eSk7b8fPAIfNw0fHKxe4rSPk3sy2GihJUFRdJWxWKrx8pIdEhC0HbBlEbIpZCAa6Ncf4ZAWTPPAQeNeV0QZAuljK13T70E8ZClbZAyDmdLVGv5saZByhfZBmtlCJBbKw9vnZCfOtxeaF5kzcHWiQiK2wrGl";
        
        // db interactions
        private string updateDBQuery = @"
INSERT INTO datadump.Tweets 
	(Username, TweetDate, TweetText, SearchHashtag, Hashtags, RetweetCount, InsertRun)
VALUES ";

        public FacebookReader()
        {
            client = new FacebookClient(accessToken);
        }


        public void Run()
        {
            string result = client.Get("https://graph.facebook.com/WREMOnz/posts?since=last+year").ToString();
            FacebookResult data = JsonConvert.DeserializeObject<FacebookResult>(result);
            data.Data = data.Data.Where(d => d.Message != null && d.Message.ToLower().Contains("warning")).ToList<FacebookData>();
            Console.WriteLine(data.Data.Count + " posts found");
        }


        private void WriteToDatabase(FacebookResult data)
        {

        }
    }
}
