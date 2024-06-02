using System.Collections.Generic;
using System.Linq;

namespace WindowsSocketCore
{
    public class Request
    {
        public string Url { get; }
        public string Body { get; }
        public string Verb { get; }
        public KeyValuePair<string, string>[] Headers { get; }

        public bool IsWebsocketUpgrade => Verb.ToUpper() == "GET" && Headers.Any(h => h.Key.ToUpper() == "CONNECTION" && h.Value.ToUpper().Contains("UPGRADE"));
        public bool IsWebsiteRequest => Verb.ToUpper() == "GET" && !Headers.Any(h => h.Key.ToUpper() == "CONNECTION" && h.Value.ToUpper().Contains("UPGRADE"));

        public Request(string url, string body, string verb, KeyValuePair<string, string>[] headers)
        {
            Url = url;
            Body = body;
            Verb = verb;
            Headers = headers;
        }

        public static Request Parse(string rawRequestStr)
        {
            var lines = rawRequestStr.Split("\r\n");
            var firstLineTokens = lines[0].Split(" ");

            var headers = lines.Skip(1).TakeWhile(l => l != "").Select(h => 
            {
                var headerTokens = h.Split(":");
                return new KeyValuePair<string,string>(headerTokens[0].Trim(), headerTokens[1].Trim());
            }).ToArray();
            var bodyLines = lines.SkipWhile(l => l != "").ToArray();
            
            return new Request(
                url: firstLineTokens[1],
                body: bodyLines.FirstOrDefault(l => l != ""),
                verb: firstLineTokens[0],
                headers: headers
            );
        }
    }
}