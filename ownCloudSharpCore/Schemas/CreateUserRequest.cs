using System;
namespace owncloudsharp.Schemas
{
    public class CreateUserRequest
    {
        public string userid { get; set; }
        public string password { get; set; }
        public string[] groups { get; set; }
    }
}
