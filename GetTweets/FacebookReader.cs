using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;

namespace GetTweets
{
    class FacebookReader
    {
        public FacebookClient client;

        public string accessToken;
        private string db_connection_string = "Data Source=txpxz2472h.database.windows.net;Initial Catalog=TremorzGovHack2015;Database=TremorzGovHack2015;User Id=TremorzAdmin;Password=Tremorz@2015";

        // db interactions
        private string updateDBQuery = @"
INSERT INTO datadump.FacebookPosts
	(UpdatedDate, [Message], [Source])
VALUES ";

        public FacebookReader()
        {
            SetAccessToken();
            client = new FacebookClient(accessToken);
        }

        public void Run()
        {
            FacebookResult result = GetData();
            WriteToDatabase(result);
        }


        private void SetAccessToken()
        {
            client = new FacebookClient();
            string result = client.Get("https://graph.facebook.com/oauth/access_token?client_id=706279499513732&client_secret=d9b769b428ea44927913f3f912a45061&grant_type=client_credentials").ToString();

            Dictionary<string,string> dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(result);
            accessToken = dict["access_token"];

            /*
            /oauth/access_token?
     client_id={app-id}
    &client_secret={app-secret}
    &grant_type=client_credentials*/
        }

        private FacebookResult GetData()
        {
            string result = client.Get("https://graph.facebook.com/WREMOnz/posts?since=last+year").ToString();
            FacebookResult data = JsonConvert.DeserializeObject<FacebookResult>(result);
            data.Data = data.Data.Where(
                    d => d.Message != null 
                    && (d.Message.ToLower().Contains("warning")
                        || d.Message.ToLower().Contains("emergency"))).ToList<FacebookData>();
            return data;
        }

        private void WriteToDatabase(FacebookResult posts)
        {
            if (posts.Data.Count == 0)
                return;

            StringBuilder updateQuery = new StringBuilder(updateDBQuery);
            foreach (FacebookData post in posts.Data)
            {
                updateQuery.Append("(");
                updateQuery.Append("'" + DateTime.ParseExact(post.Updated_Time, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd hh:mm:ss") + "'");
                updateQuery.Append(",'" + post.Message.Replace("'","''") + "'");
                updateQuery.Append(",'WREMOnz'");
                updateQuery.Append("),");
            }
            string dbQuery = updateQuery.Remove(updateQuery.Length - 1, 1).ToString();

            SqlConnection con = new SqlConnection(db_connection_string);
            SqlCommand cmd = new SqlCommand(dbQuery, con);

            cmd.CommandType = CommandType.Text;

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
