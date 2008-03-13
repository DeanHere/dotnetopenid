using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId {
	/// <summary>
	/// A trust root to validate requests and match return URLs against.
	/// </summary>
	/// <remarks>
	/// This fills the OpenID Authentication 2.0 specification for realms.
	/// See http://openid.net/specs/openid-authentication-2_0.html#realms
	/// </remarks>
	public class Realm {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public Realm(string trustRootUrl) {
			DomainWildcard = Regex.IsMatch(trustRootUrl, wildcardDetectionPattern);
			uri = new Uri(Regex.Replace(trustRootUrl, wildcardDetectionPattern, m => m.Groups[1].Value));
			if (!uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
				!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
				throw new UriFormatException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidScheme, uri.Scheme));
		}

		Uri uri;
		const string wildcardDetectionPattern = @"^(\w+://)\*\.";

		/// <summary>
		/// Whether a '*.' prefix to the hostname is used in the trustroot to allow
		/// subdomains or hosts to be added to the URL.
		/// </summary>
		public bool DomainWildcard { get; private set; }
		/// <summary>
		/// Gets the host component of this instance.
		/// </summary>
		public string Host { get { return uri.Host; } }
		/// <summary>
		/// Gets the scheme name for this URI.
		/// </summary>
		public string Scheme { get { return uri.Scheme; } }
		/// <summary>
		/// Gets the port number of this URI.
		/// </summary>
		public int Port { get { return uri.Port; } }
		/// <summary>
		/// Gets the absolute path of the URI.
		/// </summary>
		public string AbsolutePath { get { return uri.AbsolutePath; } }
		/// <summary>
		/// Gets the System.Uri.AbsolutePath and System.Uri.Query properties separated
		/// by a question mark (?).
		/// </summary>
		public string PathAndQuery { get { return uri.PathAndQuery; } }

		static string[] _top_level_domains =    {"com", "edu", "gov", "int", "mil", "net", "org", "biz", "info", "name", "museum", "coop", "aero", "ac", "ad", "ae",
			"af", "ag", "ai", "al", "am", "an", "ao", "aq", "ar", "as", "at", "au", "aw", "az", "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj",
			"bm", "bn", "bo", "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci", "ck", "cl", "cm", "cn", "co", "cr",
			"cu", "cv", "cx", "cy", "cz", "de", "dj", "dk", "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "fi", "fj", "fk", "fm", "fo",
			"fr", "ga", "gd", "ge", "gf", "gg", "gh", "gi", "gl", "gm", "gn", "gp", "gq", "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr",
			"ht", "hu", "id", "ie", "il", "im", "in", "io", "iq", "ir", "is", "it", "je", "jm", "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp",
			"kr", "kw", "ky", "kz", "la", "lb", "lc", "li", "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "mg", "mh", "mk", "ml", "mm",
			"mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "no", "np", "nr",
			"nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "ru", "rw", "sa",
			"sb", "sc", "sd", "se", "sg", "sh", "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy", "sz", "tc", "td", "tf", "tg", "th",
			"tj", "tk", "tm", "tn", "to", "tp", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "uk", "um", "us", "uy", "uz", "va", "vc", "ve", "vg", "vi",
			"vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zw"};

		/// <summary>
		/// This method checks the to see if a trust root represents a reasonable (sane) set of URLs.
		/// </summary>
		/// <remarks>
		/// 'http://*.com/', for example is not a reasonable pattern, as it cannot meaningfully 
		/// specify the site claiming it. This function attempts to find many related examples, 
		/// but it can only work via heuristics. Negative responses from this method should be 
		/// treated as advisory, used only to alert the user to examine the trust root carefully.
		/// </remarks>
		internal bool IsSane {
			get {
				if (Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
					return true;

				string[] host_parts = Host.Split('.');

				string tld = host_parts[host_parts.Length - 1];

				if (Array.IndexOf(_top_level_domains, tld) < 0)
					return false;

				if (tld.Length == 2) {
					if (host_parts.Length == 1)
						return false;

					if (host_parts[host_parts.Length - 2].Length <= 3)
						return host_parts.Length > 2;

				} else {
					return host_parts.Length > 1;
				}

				return false;
			}
		}

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">A string specifying URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool Contains(string url) {
			return Contains(new Uri(url));
		}

		/// <summary>
		/// Validates a URL against this trust root.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		/// <returns>Whether the given URL is within this trust root.</returns>
		internal bool Contains(Uri url) {
			if (url.Scheme != Scheme)
				return false;

			if (url.Port != Port)
				return false;

			if (!DomainWildcard) {
				if (url.Host != Host) {
					return false;
				}
			} else {
				Debug.Assert(!string.IsNullOrEmpty(Host), "The host part of the Regex should evaluate to at least one char for successful parsed trust roots.");
				string[] host_parts = Host.Split('.');
				string[] url_parts = url.Host.Split('.');

				// If the domain contain the wildcard has more parts than the URL to match against,
				// it naturally can't be valid.
				// Unless *.example.com actually matches example.com too.
				if (host_parts.Length > url_parts.Length)
					return false;

				// Compare last part first and move forward.
				// Could be done by using EndsWith, but this solution seems more elegant.
				for (int i = host_parts.Length - 1; i >= 0; i--) {
					/*
					if (host_parts[i].Equals("*", StringComparison.Ordinal))
					{
						break;
					}
					 */

					if (!host_parts[i].Equals(url_parts[i + 1], StringComparison.OrdinalIgnoreCase)) {
						return false;
					}
				}
			}

			// If path matches or is specified to root ...
			if (PathAndQuery.Equals(url.PathAndQuery, StringComparison.Ordinal)
				|| PathAndQuery.Equals("/", StringComparison.Ordinal))
				return true;

			// If trust root has a longer path, the return URL must be invalid.
			if (PathAndQuery.Length > url.PathAndQuery.Length)
				return false;

			// The following code assures that http://example.com/directory isn't below http://example.com/dir,
			// but makes sure http://example.com/dir/ectory is below http://example.com/dir
			int path_len = PathAndQuery.Length;
			string url_prefix = url.PathAndQuery.Substring(0, path_len);

			if (PathAndQuery != url_prefix)
				return false;

			// If trust root includes a query string ...
			if (PathAndQuery.Contains("?")) {
				// ... make sure return URL begins with a new argument
				return url.PathAndQuery[path_len] == '&';
			}

			// Or make sure a query string is introduced or a path below trust root
			return PathAndQuery.EndsWith("/", StringComparison.Ordinal)
				|| url.PathAndQuery[path_len] == '?'
				|| url.PathAndQuery[path_len] == '/';
		}

		public override bool Equals(object obj) {
			Realm other = obj as Realm;
			if (other == null) return false;
			return uri.Equals(other.uri) && DomainWildcard == other.DomainWildcard;
		}
		public override string ToString() {
			if (DomainWildcard) {
				UriBuilder builder = new UriBuilder(uri);
				builder.Host = "*." + builder.Host;
				return builder.ToString();
			} else {
				return uri.ToString();
			}
		}
	}
}