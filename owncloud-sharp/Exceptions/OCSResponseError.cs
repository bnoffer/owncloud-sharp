using System;

namespace owncloudsharp.Exceptions
{
	/// <summary>
	/// OCS API response error.
	/// </summary>
	public class OCSResponseError : ResponseError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Exceptions.OCSResponseError"/> class.
		/// </summary>
		public OCSResponseError () : base()	{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Exceptions.OCSResponseError"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public OCSResponseError (string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Exceptions.OCSResponseError"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="statusCode">Status code associated to the error.</param>
		public OCSResponseError (string message, string statusCode) : base(message, statusCode) { }
	}
}

