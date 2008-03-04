using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Collections.Specialized;


namespace DotNetOpenId.Test {
	[TestFixture]
	public class KeyValueFormEncodingTests {

		public static void KVDictTest(byte[] kvform, IDictionary<string, string> dict) {
			var d = ProtocolMessages.KeyValueForm.GetDictionary(kvform);

			foreach (string key in dict.Keys) {
				Assert.AreEqual(d[key], dict[key], d[key] + " and " + dict[key] + " do not match.");
			}
		}

		[Test]
		public void EncodeDecode() {

			KVDictTest(UTF8Encoding.UTF8.GetBytes(""), new Dictionary<string, string>());

			Dictionary<string, string> d1 = new Dictionary<string, string>();
			d1.Add("college", "harvey mudd");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("college:harvey mudd\n"), d1);


			Dictionary<string, string> d2 = new Dictionary<string, string>();
			d2.Add("city", "claremont");
			d2.Add("state", "CA");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("city:claremont\nstate:CA\n"), d2);

			Dictionary<string, string> d3 = new Dictionary<string, string>();
			d3.Add("is_valid", "true");
			d3.Add("invalidate_handle", "{HMAC-SHA1:2398410938412093}");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("is_valid:true\ninvalidate_handle:{HMAC-SHA1:2398410938412093}\n"), d3);

			Dictionary<string, string> d4 = new Dictionary<string, string>();
			d4.Add("", "");
			KVDictTest(UTF8Encoding.UTF8.GetBytes(":\n"), d4);

			Dictionary<string, string> d5 = new Dictionary<string, string>();
			d5.Add("", "missingkey");
			KVDictTest(UTF8Encoding.UTF8.GetBytes(":missingkey\n"), d5);

			Dictionary<string, string> d6 = new Dictionary<string, string>();
			d6.Add("street", "foothill blvd");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("street:foothill blvd\n"), d6);

			Dictionary<string, string> d7 = new Dictionary<string, string>();
			d7.Add("major", "computer science");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("major:computer science\n"), d7);

			Dictionary<string, string> d8 = new Dictionary<string, string>();
			d8.Add("dorm", "east");
			KVDictTest(UTF8Encoding.UTF8.GetBytes(" dorm : east \n"), d8);

			Dictionary<string, string> d9 = new Dictionary<string, string>();
			d9.Add("e^(i*pi)+1", "0");
			KVDictTest(UTF8Encoding.UTF8.GetBytes("e^(i*pi)+1:0"), d9);

			Dictionary<string, string> d10 = new Dictionary<string, string>();
			d10.Add("east", "west");
			d10.Add("north", "south");

			KVDictTest(UTF8Encoding.UTF8.GetBytes("east:west\nnorth:south"), d10);
		}

		void illegal(string s, KeyValueFormConformanceLevel level) {
			new KeyValueFormEncoding(level).GetDictionary(Encoding.UTF8.GetBytes(s));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void NoValue() {
			illegal("x\n", KeyValueFormConformanceLevel.OpenID11);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void NoValueLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			KVDictTest(Encoding.UTF8.GetBytes("x\n"), d);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void EmptyLine() {
			illegal("x:b\n\n", KeyValueFormConformanceLevel.OpenID20);
		}

		[Test]
		public void EmptyLineLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			d.Add("x", "b");
			KVDictTest(Encoding.UTF8.GetBytes("x:b\n\n"), d);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void LastLineNotTerminated() {
			illegal("x:y\na:b", KeyValueFormConformanceLevel.OpenID11);
		}

		[Test]
		public void LastLineNotTerminatedLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			d.Add("x", "y");
			d.Add("a", "b");
			KVDictTest(Encoding.UTF8.GetBytes("x:y\na:b"), d);
		}
	}
}