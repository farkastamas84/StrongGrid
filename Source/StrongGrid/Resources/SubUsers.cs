using Newtonsoft.Json.Linq;
using Pathoschild.Http.Client;
using StrongGrid.Model;
using StrongGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StrongGrid.Resources
{
	/// <summary>
	/// Allows you to manage sub-users.
	/// </summary>
	/// <remarks>
	/// See https://sendgrid.com/docs/API_Reference/Web_API_v3/subusers.html
	/// </remarks>
	public class SubUsers
	{
		private const string _endpoint = "subusers";
		private readonly Pathoschild.Http.Client.IClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="SubUsers" /> class.
		/// </summary>
		/// <param name="client">SendGrid Web API v3 client</param>
		public SubUsers(Pathoschild.Http.Client.IClient client)
		{
			_client = client;
		}

		public Task<User[]> GetAllAsync(int limit = 25, int offset = 0, CancellationToken cancellationToken = default(CancellationToken))
		{
			return _client
				.GetAsync(_endpoint)
				.WithArgument("limit", limit)
				.WithArgument("offset", offset)
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<User[]>();
		}
	}
}
