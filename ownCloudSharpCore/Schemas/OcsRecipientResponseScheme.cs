using System;
using System.Collections.Generic;

namespace owncloudsharp.Schemas
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Value
    {
        public int shareType { get; set; }
        public string shareWith { get; set; }
        public string shareWithAdditionalInfo { get; set; }
    }

    public class User
    {
        public string label { get; set; }
        public Value value { get; set; }
    }

    public class Exact
    {
        public List<object> groups { get; set; }
        public List<object> remotes { get; set; }
        public List<User> users { get; set; }
    }

    public class Group
    {
        public string label { get; set; }
        public Value value { get; set; }
    }

    public class Data
    {
        public Exact exact { get; set; }
        public List<Group> groups { get; set; }
        public List<object> remotes { get; set; }
        public List<User> users { get; set; }
    }

    public class OcsRecipientResponse
    {
        public Data data { get; set; }
        public OcsMetaResponseScheme meta { get; set; }
    }

    public class OcsRecipientResponseScheme
    {
        public OcsRecipientResponse ocs { get; set; }
    }
}
