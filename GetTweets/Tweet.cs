using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetTweets
{
    // JSON classes for twitter results
    public class TweetsList
    {
        public List<Tweet> Statuses { get; set; }
    }

    public class Tweet
    {
        public string Id { get; set; }
        public TweetUser User { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public string Created_At { get; set; }
        public string Text { get; set; }
        public string Retweet_Count { get; set; }
        public Dictionary<string,dynamic> Geo { get; set; }
        public TweetEntities Entities { get; set; }
    }

    public class TweetUser
    {
        public string Screen_Name { get; set; }
    }

    public class TweetEntities {
        public List<Hashtag> Hashtags {get; set;}
    }

    public class Hashtag {
        public string Text {get; set;}
        public List<string> Indices { get; set;}
    }
}
