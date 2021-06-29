using System;
using System.Collections.Generic;

namespace owncloudsharp.Schemas
{
    public class Meta
    {
        public string status { get; set; }
        public int statuscode { get; set; }
        public object message { get; set; }
        public string totalitems { get; set; }
        public string itemsperpage { get; set; }
    }

    public class OwnCloud
    {
        public bool read { get; set; }
        public bool create { get; set; }
        public bool update { get; set; }
        public bool delete { get; set; }
    }

    public class Permissions
    {
        public OwnCloud ownCloud { get; set; }
    }

    public class PublicLinks
    {
        public string displayDescription { get; set; }
        public int order { get; set; }
        public List<string> resourceTypes { get; set; }
        public Permissions permissions { get; set; }
    }

    public class Context
    {
        public PublicLinks publicLinks { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public int share_type { get; set; }
        public string uid_owner { get; set; }
        public string displayname_owner { get; set; }
        public int permissions { get; set; }
        public int stime { get; set; }
        public object parent { get; set; }
        public object expiration { get; set; }
        public object token { get; set; }
        public string uid_file_owner { get; set; }
        public string displayname_file_owner { get; set; }
        public int state { get; set; }
        public string path { get; set; }
        public string item_type { get; set; }
        public string mimetype { get; set; }
        public string storage_id { get; set; }
        public int storage { get; set; }
        public int item_source { get; set; }
        public int file_source { get; set; }
        public int file_parent { get; set; }
        public string file_target { get; set; }
        public string share_with { get; set; }
        public string share_with_displayname { get; set; }
        public object share_with_additional_info { get; set; }
        public int mail_send { get; set; }
        public object attributes { get; set; }
        public Context context { get; set; }
    }

    public class Ocs
    {
        public Meta meta { get; set; }
        public List<Datum> data { get; set; }
    }

    public class OcsResponseSchema
    {
        public Ocs ocs { get; set; }
    }
}
