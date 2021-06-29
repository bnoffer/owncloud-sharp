using System;
namespace owncloudsharp.Schemas
{
    public class OcsMetaResponseScheme
    {
        public string status { get; set; }
        public int statuscode { get; set; }
        public object message { get; set; }
        public string totalitems { get; set; }
        public string itemsperpage { get; set; }
    }
}
