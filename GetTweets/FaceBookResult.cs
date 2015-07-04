using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetTweets
{
    class FacebookResult
    {
        public List<FacebookData> Data { get; set; }    
    }

    public class FacebookData {
        public string Id {get; set;}
        public string Message { get; set; }
    }
}
