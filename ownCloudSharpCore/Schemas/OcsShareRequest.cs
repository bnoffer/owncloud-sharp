using System;
namespace owncloudsharp.Schemas
{
    public class OcsShareRequest
    {
        public string name { get; set; }
        public string path { get; set; }
        public int? shareType { get; set; }
        public string shareWith { get; set; }
        public bool? publicUpload { get; set; }
        public string password { get; set; }
        public int? permissions { get; set; }
        public string expireDate { get; set; }
        public object[] attributes { get; set; }
    }
}
