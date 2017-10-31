using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;

namespace SlackCatsWeb
{
    static class Bot
    {
        static Bot()
        {
            Log = new StringBuilder();

            Log.AppendLine("SlackCats v0.6");
            Log.AppendLine();

            Console.WriteLine(Log);

            var thread = new Thread(new ThreadStart(Meow));

            thread.Start();
        }

        static public StringBuilder Log
        {
            get;
            private set;
        }

        static private void Meow()
        {
            var client = new WebClient();
            var hash = new HashSet<string>();

            while (true)
            {
                Thread.Sleep(5000);

                var download = Environment.GetEnvironmentVariable("SlackCats_URL_Download");

                if (String.IsNullOrEmpty(download))
                {
                    Log.AppendLine("Missing Environment Variable: SlackCats_URL_Download");
                    continue;
                }

                var json = client.DownloadString(download);

                var data = JsonConvert.DeserializeObject<History>(json);

                if(data == null)
                {
                    Log.AppendLine("NO DATA!");
                    continue;
                }

                if (!data.ok)
                {
                    Log.AppendLine("BAD DATA!");
                    continue;
                }

                var upload = Environment.GetEnvironmentVariable("SlackCats_URL_Upload");

                System.Array.Reverse(data.messages);

                for (int i = 0; i < data.messages.Length; i++)
                {
                    var ts = data.messages[i].ts;

                    if (hash.Contains(ts))
                    {
                        continue;
                    }

                    hash.Add(ts);

                    var text = data.messages[i].text;
                    var user = data.messages[i].user;
                    var test = "test";

                    Log.AppendLine(String.Format("{0} {1} {2}", user, text, test));

                    if (String.IsNullOrEmpty(upload))
                    {
                        Log.AppendLine("Missing Environment Variable: SlackCats_URL_Upload");
                        continue;
                    }

                    if (!text.Contains("meow") && !Regex.IsMatch(text, "m+e+o*w+", RegexOptions.IgnoreCase))
                    {
                        continue;
                    }

                    Thread.Sleep(100);

                    Log.AppendLine("SENDING MEOW!");

                    json = JsonConvert.SerializeObject(new
                    {
                        text = ":cat:",
                        username = "SlackMeower",
                        icon_emoji = ":cat:"
                    });

                    var status = client.UploadString(upload, "POST", json);

                    if (status == "ok")
                    {
                        continue;
                    }

                    Log.AppendLine("CHAT FAILED!");
                }
            }
        }

        // Chat History JSON Structure

        class History
        {
            public bool ok;
            public Message[] messages;
        }

        class Message
        {
            public string ts;
            public string text;
            public string user;
        }
    }
}