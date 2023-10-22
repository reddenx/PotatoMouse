using System.Collections.Generic;
using System.Linq;

namespace WindowsSocketCore
{
    public class Request
    {
        public string Url { get; internal set; }
        public string Body { get; internal set; }
        public string Verb { get; internal set; }
        public KeyValuePair<string, string>[] Headers { get; internal set; }

        public bool IsWebsocketUpgrade => Verb.ToUpper() == "GET" && Headers.Any(h => h.Key.ToUpper() == "CONNECTION" && h.Value.ToUpper() == "UPGRADE");
        public bool IsWebsiteRequest => Verb.ToUpper() == "GET" && !Headers.Any(h => h.Key.ToUpper() == "CONNECTION" && h.Value.ToUpper() == "UPGRADE");
    }
}