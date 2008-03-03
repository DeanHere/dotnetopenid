﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Consumer;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndTesting {
		IConsumerApplicationStore appStore;

		[SetUp]
		public void Setup() {
			appStore = new ConsumerApplicationMemoryStore();
		}

		void parameterizedTest(Uri identityUrl, TrustRoot trustRoot, Uri returnTo,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			var store = provideStore ? appStore : null;

			var consumer = new OpenIdConsumer(new NameValueCollection(), store);

			Assert.IsNull(consumer.Response);
			var request = consumer.CreateRequest(identityUrl, trustRoot, returnTo);

			// Test properties and defaults
			Assert.AreEqual(AuthenticationRequestMode.Setup, request.Mode);
			Assert.AreEqual(returnTo, request.ReturnToUrl);
			Assert.AreEqual(trustRoot.Url, request.TrustRootUrl.Url);

			request.Mode = requestMode;

			// Verify the redirect URL
			Assert.IsNotNull(request.RedirectToProviderUrl);
			var consumerToProviderQuery = HttpUtility.ParseQueryString(request.RedirectToProviderUrl.Query);
			Assert.IsTrue(consumerToProviderQuery[QueryStringArgs.openid.return_to].StartsWith(returnTo.AbsoluteUri, StringComparison.Ordinal));
			Assert.AreEqual(trustRoot.Url, consumerToProviderQuery[QueryStringArgs.openid.trust_root]);

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(request.RedirectToProviderUrl);
			providerRequest.AllowAutoRedirect = false;
			Uri redirectUrl;
			using (HttpWebResponse providerResponse = (HttpWebResponse)providerRequest.GetResponse()) {
				Assert.AreEqual(HttpStatusCode.Redirect, providerResponse.StatusCode);
				redirectUrl = new Uri(providerResponse.Headers[HttpResponseHeader.Location]);
			}
			var providerToConsumerQuery = HttpUtility.ParseQueryString(redirectUrl.Query);
			var consumer2 = new OpenIdConsumer(providerToConsumerQuery, store);
			Assert.AreEqual(expectedResult, consumer2.Response.Status);
			Assert.AreEqual(identityUrl, consumer2.Response.IdentityUrl);

			// Try replay attack
			if (tryReplayAttack) {
				// This simulates a network sniffing user who caught the 
				// authenticating query en route to either the user agent or
				// the consumer, and tries the same query to the consumer in an
				// attempt to spoof the identity of the authenticating user.
				try {
					var replayAttackConsumer = new OpenIdConsumer(providerToConsumerQuery, store);
					Assert.AreNotEqual(AuthenticationStatus.Authenticated, replayAttackConsumer.Response.Status, "Replay attack");
				} catch (OpenIdException) { // nonce already used
					// another way to pass
				}
			}
		}

		[Test]
		public void Pass_Setup_AutoApproval() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval),
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}

		[Test]
		public void Pass_Immediate_AutoApproval() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval),
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}

		[Test]
		public void Fail_Immediate_ApproveOnSetup() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup),
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired,
				false,
				true
			);
		}

		[Test]
		public void Pass_Setup_ApproveOnSetup() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup),
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}
	}
}
