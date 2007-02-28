using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.Yadis
{
    [Serializable]
    public class Yadis
    {
        private const string CONTENT_TYPE_HTML = "text/html";
        private const string CONTENT_TYPE_XHTML = "application/xhtml+xml";
        public const string CONTENT_TYPE = "application/xrds+xml";
        protected const string HEADER_NAME = "X-XRDS-Location";
        public static readonly string ACCEPT_HEADER;

        static Yadis()
        {
            //TODO: SDH - I'm not done...
            //ACCEPT_HEADER = AcceptHeader.Generate(
            //    new object[]
            //    {
            //        new object[] { CONTENT_TYPE_HTML, 0.3 }, 
            //        new object[] { CONTENT_TYPE_XHTML, 0.5 },
            //        new object[] { CONTENT_TYPE, 1 }
            //    }
            //);
        }

        public static DiscoveryResult Discover(Uri uri)
        {
            FetchRequest request = new FetchRequest(uri);
            request.Request.Accept = ACCEPT_HEADER;
            FetchResponse response = request.GetResponse(true);
            if (((int)response.StatusCode) != 200)
            {
                return null;
            }
            FetchResponse response2 = null;
            if (response.ContentType.MediaType == CONTENT_TYPE)
            {
                response2 = response;
            }
            else
            {
                string uriString = response.Headers.Get(HEADER_NAME.ToLower());
                Uri url = null;
                try
                {
                    url = new Uri(uriString);
                }
                catch (UriFormatException)
                {
                    url = null;
                }
                catch (ArgumentNullException)
                {
                    url = null;
                }
                if ((url == null) && (response.ContentType.MediaType == CONTENT_TYPE_HTML))
                {
                    url = MetaYadisLoc(response.Body);
                }
                if (url != null)
                {
                    response2 = new FetchRequest(url).GetResponse(false);
                    if (((int)response2.StatusCode) != 200)
                    {
                        return null;
                    }
                }
            }
            return new DiscoveryResult(uri, response, response2);
        }

        public static Uri MetaYadisLoc(string html)
        {
            object[] objArray = ByteParser.HeadTagAttrs(html, "meta");
            foreach(NameValueCollection values in objArray)
            {
                string text = values["http-equiv"];
                if ((text != null) && (text.ToLower() == HEADER_NAME.ToLower()))
                {
                    string uriString = values.Get("content");
                    if (uriString != null)
                    {
                        try
                        {
                            return new Uri(uriString);
                        }
                        catch (UriFormatException)
                        {
                            continue;
                        }
                    }
                }
            }
            return null;
        }
    }

    [Serializable]
    public class DiscoveryResult
    {
        // Fields
        protected ContentType contentType;
        protected Uri normalizedUri;
        protected Uri requestUri;
        protected string responseText;
        protected Uri yadisLocation;

        // Methods
        public DiscoveryResult(Uri requestUri, FetchResponse initResp, FetchResponse finalResp)
        {
            this.requestUri = requestUri;
            this.normalizedUri = initResp.FinalUri;
            if (finalResp == null)
            {
                this.contentType = initResp.ContentType;
                this.responseText = initResp.Body;
            }
            else
            {
                this.contentType = finalResp.ContentType;
                this.responseText = finalResp.Body;
            }
            if ((initResp != finalResp) && (finalResp != null))
            {
                this.yadisLocation = finalResp.RequestUri;
            }
        }

        // Properties
        public ContentType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public bool IsXRDS
        {
            get
            {
                return (this.UsedYadisLocation || (this.contentType.MediaType == "application/xrds+xml"));
            }
        }

        public Uri NormalizedUri
        {
            get
            {
                return this.normalizedUri;
            }
        }

        public Uri RequestUri
        {
            get
            {
                return this.requestUri;
            }
        }

        public string ResponseText
        {
            get
            {
                return this.responseText;
            }
        }

        public bool UsedYadisLocation
        {
            get
            {
                return (this.yadisLocation != null);
            }
        }

        public Uri YadisLocation
        {
            get
            {
                return this.yadisLocation;
            }
        }
    }

 
 

}
