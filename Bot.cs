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
            Download = Environment.GetEnvironmentVariable("SlackCats_URL_Download");
            Upload = Environment.GetEnvironmentVariable("SlackCats_URL_Upload");

            Log = new StringBuilder();

            var thread = new Thread(new ThreadStart(Meow));

            thread.Start();
        }

        static public string Download
        {
            get;
            set;
        }

        static public StringBuilder Log
        {
            get;
            private set;
        }

        static public string Upload
        {
            get;
            set;
        }

        static private void Meow()
        {
            var client = new WebClient();
            var hash = new HashSet<string>();

            while (true)
            {
                if (String.IsNullOrEmpty(Download))
                {
                    continue;
                }

                var json = client.DownloadString(Download);

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

                    Log.AppendLine(String.Format("{0} {1}", user, text));

                    if (String.IsNullOrEmpty(Upload))
                    {
                        continue;
                    }

                    if (!Regex.IsMatch(text, "m+e+o*w+", RegexOptions.IgnoreCase))
                    {
                        continue;
                    }

                    json = JsonConvert.SerializeObject(new
                    {
                        text = ":cat:",
                        username = "SlackMeower",
                        icon_emoji = ":cat:"
                    });

                    client.UploadString(Upload, "POST", json);
                }

                Thread.Sleep(5000);
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