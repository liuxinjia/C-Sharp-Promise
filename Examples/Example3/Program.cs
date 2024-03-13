using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

//
// This example downloads search results from google then transforms the result to extract links.
// It includes both error handling and a completion handler.
//
namespace Example
{
    class Program
    {
        //
        // URL for a google search on 'promises'.
        //
        static string searchUrl = "https://www.google.com/#q=promises";

        static void Main(string[] args)
        {
            var running = true;

            Download(searchUrl)         // Invoke a google search.
                .Then(html =>           // Transforms search results and extract links.
                {
                    return LinkFinder
                        .Find(html)
                        .Select(link => link.Href)
                        .ToArray();
                })
                .Then(links =>          // Display the links that were extracted.
                {
                    Console.WriteLine("Async operation completed.");
                    foreach (var link in links)
                    {
                        Console.WriteLine(link);
                    }
                    running = false;
                })            
                .Catch(exception =>     // Catch any errors that happen during download or transform.
                {
                    Console.WriteLine("Async operation errorred.");
                    Console.WriteLine(exception);
                    running = false;
                })
                .Done();

            Console.WriteLine("Waiting");

            while (running)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("Exiting");
        }

        /// <summary>
        /// Download text from a URL.
        /// A promise is returned that is resolved when the download has completed.
        /// The promise is rejected if an error occurs during download.
        /// </summary>
        static IPromise<string> Download(string url)
        {
            Console.WriteLine("Downloading " + url + " ...");

            var promise = Promise<string>.Create();
            using (var client = new WebClient())
            {
                client.DownloadStringCompleted +=
                    (s, ev) =>
                    {
                        if (ev.Error != null)
                        {
                            Console.WriteLine("An error occurred... rejecting the promise.");

                            // Error during download, reject the promise.
                            promise.Reject(ev.Error);
                        }
                        else
                        {
                            Console.WriteLine("... Download completed.");

                            // Downloaded completed successfully, resolve the promise.
                            promise.Resolve(ev.Result);
                        }
                    };

                client.DownloadStringAsync(new Uri(url), null);
            }
            return promise;
        }

        //
        // LinkItem and LinkFinder from this site:
        //
        // http://www.dotnetperls.com/scraping-html
        // 

        public struct LinkItem
        {
            public string Href;
            public string Text;

            public override string ToString()
            {
                return Href + "\n\t" + Text;
            }
        }

        static class LinkFinder
        {
            public static List<LinkItem> Find(string file)
            {
                List<LinkItem> list = new List<LinkItem>();

                // 1.
                // Find all matches in file.
                MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                    RegexOptions.Singleline);

                // 2.
                // Loop over each match.
                foreach (Match m in m1)
                {
                    string value = m.Groups[1].Value;
                    LinkItem i = new LinkItem();

                    // 3.
                    // Get href attribute.
                    Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                    RegexOptions.Singleline);
                    if (m2.Success)
                    {
                        i.Href = m2.Groups[1].Value;
                    }

                    // 4.
                    // Remove inner tags from text.
                    string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                    RegexOptions.Singleline);
                    i.Text = t;

                    list.Add(i);
                }
                return list;
            }
        }
    }
}
