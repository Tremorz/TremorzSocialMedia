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
    class Program
    {

        static void Main(string[] args)
        {
            FacebookReader fb = new FacebookReader();
            TwitterReader twitter = new TwitterReader();

            fb.Run();
            Console.WriteLine("Facebook data loaded. Twitter periodic load commencing. Press ENTER (and wait up to 30 seconds) to stop running.");

            bool running = true;
            for (int i = 0; running; i++)
            {
                twitter.Run(i);

                Console.WriteLine("Twitter data updated: Run " + i + " completed");
                if (Console.KeyAvailable)
                    running = false;

                Thread.Sleep(30000);
            }
            
        }
    }
}
