using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace owncloudsharp.Types
{
	/// <summary>
	/// OCS API Response.
	/// </summary>
	public class OCS
	{
		/// <summary>
		/// Gets or sets the meta information.
		/// </summary>
		/// <value>The meta.</value>
		public Meta Meta { get; set; }
		/// <summary>
		/// Gets or sets the data payload.
		/// </summary>
		/// <value>The data.</value>
		public object Data { get; set; }

        /// <summary>
        /// Converts a dynamic JSON object to a OCS instance
        /// </summary>
        /// <returns>OCS object instance.</returns>
        /// <param name="json">JSON to convert.</param>
        /// <param name="expectedData">Expected data type for the Data property.</param>
        public static OCS JSonDeserialize(dynamic json, Type expectedData) {
            return new OCS()
            {
                Meta = Meta.JSonDeserialize(json.ocs),
                Data = JSonDeserializeData(json.ocs, expectedData)
            };
        }

        /// <summary>
        /// Converts a dynamic JSON object to a Data property instance
        /// </summary>
        /// <returns>The deserialize data.</returns>
        /// <param name="json">JSON to convert.</param>
        /// <param name="expectedData">Expected data type.</param>
        private static Object JSonDeserializeData(dynamic json, Type expectedData) {
            if (json.data == null)
                return null;
            
            Object data = null;

            try
            {
                if (expectedData == typeof(User))
                    data = User.JSonDeserialize(json.data);
            } catch (Exception ex) {
                Debug.WriteLine("JSonDeserializeData: Parsing {0}", expectedData.ToString());
            }

            return data;
        }
	}

	/// <summary>
	/// OCS API Meta information.
	/// </summary>
	public class Meta {
		/// <summary>
		/// Gets or sets the response status.
		/// </summary>
		/// <value>The status.</value>
        public string Status { get; set; }
		/// <summary>
		/// Gets or sets the response status code.
		/// </summary>
		/// <value>The status code.</value>
        public int StatusCode { get; set; }
		/// <summary>
		/// Gets or sets the response status message.
		/// </summary>
		/// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Converts a dynamic JSON object to a Meta instance
        /// </summary>
        /// <returns>Meta object instance.</returns>
        /// <param name="json">JSON to convert.</param>
        public static Meta JSonDeserialize(dynamic json)
        {
            return new Meta()
            {
                Status = json.meta.status,
                StatusCode = json.meta.statuscode,
                Message = json.meta.message
            };
        }
	}
}

