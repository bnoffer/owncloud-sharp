using System;
namespace owncloudsharp.Schemas
{
    public class OcsCreateUserRequest
    {
        public string userid { get; set; }
        public string password { get; set; }
        public string[] groups { get; set; }
    }
}
