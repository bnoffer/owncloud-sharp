using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owncloudsharp.Types
{
    /// <summary>
    /// Provides basic information of a ownCloud Share
    /// </summary>
    public class Share
    {
        /// <summary>
        /// The shares Id assigned by ownCloud
        /// </summary>
        public string ShareId { get; set; }
        /// <summary>
        /// The path to the target file/folder
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// The permissions granted on the share
        /// </summary>
        public int Perms { get; set; }
    }
}
