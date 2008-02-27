﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using DotNetOpenId.Provider;
using DotNetOpenId.Consumer;

namespace DotNetOpenId.RegistrationExtension {
	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
	public struct ProfileRequestFields {
		public static readonly ProfileRequestFields None = new ProfileRequestFields();

		public ProfileRequest Nickname { get; set; }
		public ProfileRequest Email { get; set; }
		public ProfileRequest FullName { get; set; }
		public ProfileRequest BirthDate { get; set; }
		public ProfileRequest Gender { get; set; }
		public ProfileRequest PostalCode { get; set; }
		public ProfileRequest Country { get; set; }
		public ProfileRequest Language { get; set; }
		public ProfileRequest TimeZone { get; set; }

		/// <summary>
		/// The URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		public Uri PolicyUrl { get; set; }

		/// <summary>
		/// Sets the profile request properties according to a list of
		/// field names that might have been passed in the OpenId query dictionary.
		/// </summary>
		/// <param name="fieldNames">
		/// The list of field names that should receive a given 
		/// <paramref name="requestLevel"/>.  These field names should match 
		/// the OpenId specification for field names, omitting the 'openid.sreg' prefix.
		/// </param>
		/// <param name="requestLevel">The none/request/require state of the listed fields.</param>
		internal void SetProfileRequestFromList(ICollection<string> fieldNames, ProfileRequest requestLevel) {
			foreach (string field in fieldNames) {
				switch (field) {
					case QueryStringArgs.openidnp.sregnp.nickname:
						Nickname = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.email:
						Email = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.fullname:
						FullName = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.dob:
						BirthDate = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.gender:
						Gender = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.postcode:
						PostalCode = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.country:
						Country = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.language:
						Language = requestLevel;
						break;
					case QueryStringArgs.openidnp.sregnp.timezone:
						TimeZone = requestLevel;
						break;
					default:
						Trace.TraceWarning("OpenIdProfileRequest.SetProfileRequestFromList: Unrecognized field name '{0}'.", field);
						break;
				}
			}
		}
		string[] assembleProfileFields(ProfileRequest level) {
			List<string> fields = new List<string>(10);
			if (Nickname == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.nickname);
			if (Email == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.email);
			if (FullName == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.fullname);
			if (BirthDate == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.dob);
			if (Gender == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.gender);
			if (PostalCode == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.postcode);
			if (Country == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.country);
			if (Language == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.language);
			if (TimeZone == level)
				fields.Add(QueryStringArgs.openidnp.sregnp.timezone);

			return fields.ToArray();
		}
		/// <summary>
		/// Reads the sreg extension information on an authentication request to the provider
		/// and returns information on what profile fields the consumer is requesting/requiring.
		/// </summary>
		public static ProfileRequestFields ReadFromRequest(Request request) {
			ProfileRequestFields fields = new ProfileRequestFields();
			var args = request.GetExtensionArguments(QueryStringArgs.openidnp.sreg.Prefix.TrimEnd('.'));

			string policyUrl;
			if (args.TryGetValue(QueryStringArgs.openidnp.sregnp.policy_url, out policyUrl)
				&& !string.IsNullOrEmpty(policyUrl)) {
				fields.PolicyUrl = new Uri(policyUrl);
			}

			string optionalFields;
			if (args.TryGetValue(QueryStringArgs.openidnp.sregnp.optional, out optionalFields)) {
				fields.SetProfileRequestFromList(optionalFields.Split(','), ProfileRequest.Request);
			}

			string requiredFields;
			if (args.TryGetValue(QueryStringArgs.openidnp.sregnp.required, out requiredFields)) {
				fields.SetProfileRequestFromList(requiredFields.Split(','), ProfileRequest.Require);
			}

			return fields;
		}
		public void AddToRequest(AuthenticationRequest request) {
			var fields = new Dictionary<string, string>();
			if (PolicyUrl != null)
				fields.Add(QueryStringArgs.openidnp.sregnp.policy_url, PolicyUrl.AbsoluteUri);

			fields.Add(QueryStringArgs.openidnp.sregnp.required, string.Join(",", assembleProfileFields(ProfileRequest.Require)));
			fields.Add(QueryStringArgs.openidnp.sregnp.optional, string.Join(",", assembleProfileFields(ProfileRequest.Request)));

			request.AddExtensionArguments(QueryStringArgs.openidnp.sreg.Prefix.TrimEnd('.'), fields);
		}

		public override string ToString() {
			return string.Format(CultureInfo.CurrentUICulture, @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'", Nickname, Email, FullName, BirthDate, Gender, PostalCode, Country, Language, TimeZone);
		}
		public override bool Equals(object obj) {
			if (!(obj is ProfileRequestFields)) return false;
			ProfileRequestFields other = (ProfileRequestFields)obj;

			return
				safeEquals(this.BirthDate, other.BirthDate) &&
				safeEquals(this.Country, other.Country) &&
				safeEquals(this.Language, other.Language) &&
				safeEquals(this.Email, other.Email) &&
				safeEquals(this.FullName, other.FullName) &&
				safeEquals(this.Gender, other.Gender) &&
				safeEquals(this.Nickname, other.Nickname) &&
				safeEquals(this.PostalCode, other.PostalCode) &&
				safeEquals(this.TimeZone, other.TimeZone) &&
				safeEquals(this.PolicyUrl, other.PolicyUrl);
		}
		static bool safeEquals(object one, object other) {
			if (one == null && other == null) return true;
			if (one == null ^ other == null) return false;
			return one.Equals(other);
		}
	}
}
