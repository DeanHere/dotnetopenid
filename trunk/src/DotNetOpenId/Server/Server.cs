using System;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.Store;
using System.Web;


namespace DotNetOpenId.Server {
	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public class Server {
		IAssociationStore store;
		Signatory signatory;
		Encoder encoder;

		/// <summary>
		/// Constructs an OpenId server that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		public Server() : this(httpApplicationAssociationStore) { }

		/// <summary>
		/// Constructs an OpenId server that uses a given IAssociationStore.
		/// </summary>
		public Server(IAssociationStore store) {
			if (store == null) throw new ArgumentNullException("store");
			this.store = store;
			this.signatory = new Signatory(store);
			this.encoder = new SigningEncoder(signatory);
		}

		/// <summary>
		/// Decodes an incoming web request in to a <see cref="Request"/>.
		/// </summary>
		/// <param name="query">The query parameters as a dictionary with each key mapping to one value. </param>
		public Request DecodeRequest(NameValueCollection query) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Start message decoding");
			}

			if (query == null) return null;

			NameValueCollection myquery = new NameValueCollection();
			foreach (string key in query) {
				if (!String.IsNullOrEmpty(key)) {
					if (key.StartsWith(QueryStringArgs.openid.Prefix)) { myquery[key] = query[key]; }
				}
			}

			if (myquery.Count == 0) return null;

			string mode = myquery.Get(QueryStringArgs.openid.mode);
			if (mode == null)
				throw new ProtocolException(query, "No openid.mode value in query");

			if (mode == QueryStringArgs.Modes.checkid_setup) {
				CheckIdRequest request = new CheckIdRequest(this, query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in setup mode");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckIdRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.checkid_immediate) {
				CheckIdRequest request = new CheckIdRequest(this, query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in immediate mode");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckIdRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.check_authentication) {
				CheckAuthRequest request = new CheckAuthRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckAuthRequest");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("CheckAuthRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			} else if (mode == QueryStringArgs.Modes.associate) {
				AssociateRequest request = new AssociateRequest(query);

				if (TraceUtil.Switch.TraceInfo) {
					TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new AssociateRequest ");
					if (TraceUtil.Switch.TraceInfo) {
						TraceUtil.ServerTrace("AssociateRequest follows: ");
						TraceUtil.ServerTrace(request.ToString());
					}
				}

				return request;
			}

			throw new ProtocolException(query, "No decoder for openid.mode=" + mode);

		}

		/// <returns>
		/// Null if the given HttpRequest does not represent a request from an 
		/// OpenId client.  This could occur if someone just typed in an OpenID
		/// URL directly.
		/// </returns>
		public Request DecodeRequest(HttpRequest request) {
			return DecodeRequest(
				request.HttpMethod == "GET" ? request.QueryString : request.Form);
		}

		public WebResponse HandleRequest(Request request) {
			WebResponse response;
			switch (request.RequestType) {
				case RequestType.CheckAuthRequest:
					response = EncodeResponse(((CheckAuthRequest)request).Answer(signatory));
					break;
				case RequestType.AssociateRequest:
					response = EncodeResponse(((AssociateRequest)request).Answer(signatory.CreateAssociation(false)));
					break;
				case RequestType.CheckIdRequest:
				default:
					throw new ArgumentException("Unexpected Request.RequestMode value.  Use CheckIdRequest.Answer instead.", "request");
			}
			return response;
		}

		internal WebResponse EncodeResponse(IEncodable response) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Encoding response");
			}

			return encoder.Encode(response);
		}

		const string associationStoreKey = "DotNetOpenId.Server.Server.AssociationStore";
		static IAssociationStore httpApplicationAssociationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				IAssociationStore store = (IAssociationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IAssociationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new MemoryStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}
				return store;
			}
		}
	}
}
