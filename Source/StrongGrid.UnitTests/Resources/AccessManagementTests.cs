﻿using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Shouldly;
using StrongGrid.Model;
using StrongGrid.UnitTests;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace StrongGrid.Resources.UnitTests
{
	public class AccessManagementTests
	{
		#region FIELDS

		private const string ENDPOINT = "access_settings";

		private const string SINGLE_ACCESS_ENTRY_JSON = @"{
			'allowed': false,
			'auth_method': 'basic',
			'first_at': 1444087966,
			'ip': '1.1.1.1',
			'last_at': 1444406672,
			'location': 'Australia'
		}";
		private const string MULTIPLE_ACCESS_ENTRIES_JSON = @"{
			'result': [
				{
					'allowed': false,
					'auth_method': 'basic',
					'first_at': 1444087966,
					'ip': '1.1.1.1',
					'last_at': 1444406672,
					'location': 'Australia'
				},
				{
					'allowed': false,
					'auth_method': 'basic',
					'first_at': 1444087505,
					'ip': '1.2.3.48',
					'last_at': 1444087505,
					'location': 'Mukilteo, Washington'
				}
			]
		}";

		private const string SINGLE_WHITELISTED_IP_JSON = @"{
			'id': 1,
			'ip': '192.168.1.1/32',
			'created_at': 1441824715,
			'updated_at': 1441824715
		}";
		private const string MULTIPLE_WHITELISTED_IPS_JSON = @"{
			'result': [
				{
					'id': 1,
					'ip': '192.168.1.1/32',
					'created_at': 1441824715,
					'updated_at': 1441824715
				},
				{
					'id': 2,
					'ip': '192.168.1.2/32',
					'created_at': 1441824715,
					'updated_at': 1441824715
				},
				{
					'id': 3,
					'ip': '192.168.1.3/32',
					'created_at': 1441824715,
					'updated_at': 1441824715
				}
			]
		}";

		#endregion

		[Fact]
		public void Parse_AccessEntry_json()
		{
			// Arrange

			// Act
			var result = JsonConvert.DeserializeObject<AccessEntry>(SINGLE_ACCESS_ENTRY_JSON);

			// Assert
			result.ShouldNotBeNull();
			result.Allowed.ShouldBeFalse();
			result.AuthorizationMethod.ShouldBe("basic");
			result.FirstAccessOn.ShouldBe(new DateTime(2015, 10, 5, 23, 32, 46, DateTimeKind.Utc));
			result.IpAddress.ShouldBe("1.1.1.1");
			result.LatestAccessOn.ShouldBe(new DateTime(2015, 10, 9, 16, 4, 32, DateTimeKind.Utc));
			result.Location.ShouldBe("Australia");
		}

		[Fact]
		public void Parse_WhitelistedIp_json()
		{
			// Arrange

			// Act
			var result = JsonConvert.DeserializeObject<WhitelistedIp>(SINGLE_WHITELISTED_IP_JSON);

			// Assert
			result.ShouldNotBeNull();
			result.Id.ShouldBe(1);
			result.IpAddress.ShouldBe("192.168.1.1/32");
			result.CreatedOn.ShouldBe(new DateTime(2015, 9, 9, 18, 51, 55, DateTimeKind.Utc));
			result.ModifiedOn.ShouldBe(new DateTime(2015, 9, 9, 18, 51, 55, DateTimeKind.Utc));
		}

		[Fact]
		public void GetAccessHistory()
		{
			// Arrange
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Get, Utils.GetSendGridApiUri(ENDPOINT, "activity")).Respond("application/json", MULTIPLE_ACCESS_ENTRIES_JSON);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			var result = accessManagement.GetAccessHistoryAsync(20, CancellationToken.None).Result;

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
			result.ShouldNotBeNull();
			result.Length.ShouldBe(2);
		}

		[Fact]
		public void GetWhitelistedIpAddresses()
		{
			// Arrange
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Get, Utils.GetSendGridApiUri(ENDPOINT, "whitelist")).Respond("application/json", MULTIPLE_ACCESS_ENTRIES_JSON);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			var result = accessManagement.GetWhitelistedIpAddressesAsync(CancellationToken.None).Result;

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
			result.ShouldNotBeNull();
			result.Length.ShouldBe(2);
		}

		[Fact]
		public void AddIpAddressToWhitelist()
		{
			// Arrange
			var ip = "1.1.1.1";

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Post, Utils.GetSendGridApiUri(ENDPOINT, "whitelist")).Respond("application/json", MULTIPLE_WHITELISTED_IPS_JSON);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			var result = accessManagement.AddIpAddressToWhitelistAsync(ip, CancellationToken.None).Result;

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
			result.ShouldNotBeNull();
		}

		[Fact]
		public void AddIpAddressesToWhitelist()
		{
			// Arrange
			var ips = new[] { "1.1.1.1", "1.2.3.4", "5.6.7.8" };

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Post, Utils.GetSendGridApiUri(ENDPOINT, "whitelist")).Respond("application/json", MULTIPLE_WHITELISTED_IPS_JSON);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			var result = accessManagement.AddIpAddressesToWhitelistAsync(ips, CancellationToken.None).Result;

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
			result.ShouldNotBeNull();
			result.Length.ShouldBe(3);
		}

		[Fact]
		public void RemoveIpAddressFromWhitelistAsync()
		{
			// Arrange
			var id = 1111;

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Delete, Utils.GetSendGridApiUri(ENDPOINT, "whitelist", id)).Respond(HttpStatusCode.OK);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			accessManagement.RemoveIpAddressFromWhitelistAsync(id, CancellationToken.None).Wait(CancellationToken.None);

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
		}

		[Fact]
		public void RemoveIpAddressesFromWhitelistAsync()
		{
			// Arrange
			var ids = new long[] { 1111, 2222, 3333 };

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Delete, Utils.GetSendGridApiUri(ENDPOINT, "whitelist")).Respond(HttpStatusCode.OK);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			accessManagement.RemoveIpAddressesFromWhitelistAsync(ids, CancellationToken.None).Wait(CancellationToken.None);

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
		}


		[Fact]
		public void GetWhitelistedIpAddressAsync()
		{
			// Arrange
			var id = 1111;

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.Expect(HttpMethod.Get, Utils.GetSendGridApiUri(ENDPOINT, "whitelist", id)).Respond("application/json", SINGLE_WHITELISTED_IP_JSON);

			var client = Utils.GetFluentClient(mockHttp);
			var accessManagement = new AccessManagement(client);

			// Act
			var result = accessManagement.GetWhitelistedIpAddressAsync(id, CancellationToken.None).Result;

			// Assert
			mockHttp.VerifyNoOutstandingExpectation();
			mockHttp.VerifyNoOutstandingRequest();
			result.ShouldNotBeNull();
		}
	}
}
