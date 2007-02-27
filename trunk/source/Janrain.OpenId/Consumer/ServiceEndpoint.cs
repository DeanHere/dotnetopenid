using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Janrain.Yadis;

namespace Janrain.OpenId.Consumer
{
    class ServiceEndpoint
    {
        public static readonly Uri OPENID_1_0_NS = new Uri("http://openid.net/xmlns/1.0");
        public static readonly Uri OPENID_1_2_TYPE = new Uri("http://openid.net/signon/1.2");
        public static readonly Uri OPENID_1_1_TYPE = new Uri("http://openid.net/signon/1.1");
        public static readonly Uri OPENID_1_0_TYPE = new Uri("http://openid.net/signon/1.0");

        public static readonly Uri[] OPENID_TYPE_URIS = { OPENID_1_2_TYPE, 
                                    OPENID_1_1_TYPE, 
                                    OPENID_1_0_TYPE };

        private Uri _identityUrl;
        public Uri IdentityUrl
        {
            get
            {
                return _identityUrl;
            }
        }

        private Uri _serverUrl;
        public Uri ServerUrl
        {
            get
            {
                return _serverUrl;
            }
        }

        private Uri _delegateUrl;
        public Uri DelegateUrl
        {
            get
            {
                return _delegateUrl;
            }
        }

        private bool _usedYadis;
        public bool UsedYadis
        {
            get
            {
                return _usedYadis;
            }
        }

        Uri[] _typeUris;

        public Uri ServerId
        {
            get
            {
                if (this._delegateUrl == null)
                {
                    return this._identityUrl;
                }
                return this._delegateUrl;
            }
        }

        public static Uri ExtractDelgate(ServiceNode serviceNode)
        {
            XmlNamespaceManager nsmgr = serviceNode.XmlNsManager;
            nsmgr.PushScope();
            nsmgr.AddNamespace("openid", OPENID_1_0_NS.AbsoluteUri);
            XmlNodeList delegateNodes = serviceNode.Node.SelectNodes("./openid:Delegate", nsmgr);
            Uri delegateUrl = null;
            foreach (XmlNode delegateNode in delegateNodes)
            {
                try
                {
                    delegateUrl = new Uri(delegateNode.InnerXml);
                    break;
                }
                catch (UriFormatException)
                {
                    continue;
                }
            }
            nsmgr.PopScope();
            return delegateUrl;
        }

        internal ServiceEndpoint(Uri identityUrl, Uri serverUrl, Uri[] typeUris, Uri delegateUrl, bool usedYadis)
        {
            this._identityUrl = identityUrl;
            this._serverUrl = serverUrl;
            this._typeUris = typeUris;
            this._delegateUrl = delegateUrl;
            this._usedYadis = usedYadis;
        }

        //public ServiceEndpoint(Uri yadisUrl, UriNode uriNode)
        //{
        //    ServiceNode localNode = uriNode.ServiceNode;
        //    Uri[] typeUris = Array.Copy()
            
        //    Uri[] matches = 
            
        //    if ((matches.Length == 0) || (uriNode.Uri == null))
        //    {
        //        throw new ArgumentException("No matching openid type uris");
        //    }
        //    this._identityUrl = yadisUrl;
        //    this._serverUrl = uriNode.Uri;
        //    this._typeUris = _locals.___type_uris_1;
        //    this._delegateUrl = ExtractDelegate(localNode);
        //    this._usedYadis = true;
        //}

 

 


 

    }
}


/*
namespace Janrain.OpenId.Consumer

import System
import System.Collections.Specialized
import System.Xml
#import Janrain.Util
import Janrain.Yadis


class ServiceEndpoint:
    public static final OPENID_1_0_NS = Uri('http://openid.net/xmlns/1.0')
    public static final OPENID_1_2_TYPE = Uri('http://openid.net/signon/1.2')
    public static final OPENID_1_1_TYPE = Uri('http://openid.net/signon/1.1')
    public static final OPENID_1_0_TYPE = Uri('http://openid.net/signon/1.0')

    public static final OPENID_TYPE_URIS as (Uri) = (
        OPENID_1_2_TYPE,
        OPENID_1_1_TYPE,
        OPENID_1_0_TYPE,
        )

    [Getter(IdentityUrl)]
    identity_url as Uri

    [Getter(ServerUrl)]
    server_url as Uri

    [Getter(DelegateUrl)]
    delegate_url as Uri

    [Getter(UsedYadis)]
    used_yadis as bool

    type_uris as (Uri)

    ServerId:
        get:
            if self.delegate_url is null:
                return self.identity_url
            else:
                return self.delegate_url

    static def ExtractDelegate(service_node as ServiceNode):
        nsmgr = service_node.XmlNsManager
        nsmgr.PushScope()
        nsmgr.AddNamespace("openid", OPENID_1_0_NS.AbsoluteUri)
        delegate_nodes = service_node.Node.SelectNodes('./openid:Delegate', nsmgr)
        delegate_url as Uri
        for delegate_node as XmlNode in delegate_nodes:
            try:
                delegate_url = Uri(delegate_node.InnerXml)
                break
            except e as UriFormatException:
                pass

        nsmgr.PopScope()
        return delegate_url

    # Used for testing
    internal def constructor(identity_url as Uri, server_url as Uri,
                             type_uris as (Uri), delegate_url as Uri,
                             used_yadis as bool):
        self.identity_url = identity_url
        self.server_url = server_url
        self.type_uris = type_uris
        self.delegate_url = delegate_url
        self.used_yadis = used_yadis

    def constructor(yadis_url as Uri, uri_node as UriNode):
        service_node = uri_node.ServiceNode
        type_uris as (Uri) = array(Uri, type_node.Uri \
                                   for type_node in service_node.TypeNodes())

        matches = array(Uri, type_uri for type_uri in type_uris \
                        if type_uri in OPENID_TYPE_URIS)

        if not len(matches) or uri_node.Uri is null:
            raise ArgumentException("No matching openid type uris")

        self.identity_url = yadis_url
        self.server_url = uri_node.Uri
        self.type_uris = type_uris
        self.delegate_url = ExtractDelegate(service_node)
        self.used_yadis = true

    def constructor(uri as Uri, html as string):
        # Look for openid.server/delegate link rel tags
        for attrs as NameValueCollection in ByteParser.HeadTagAttrs(
            html, 'link'):
            rel = attrs["rel"]
            if rel is not null:
                href = attrs["href"]
                if href is not null:
                    if rel == "openid.server" and self.server_url is null:
                        try:
                            self.server_url = Uri(href)
                        except e as UriFormatException:
                            pass

                    if rel == "openid.delegate" and self.delegate_url is null:
                        try:
                            self.delegate_url = Uri(href)
                        except e as UriFormatException:
                            pass

        if self.server_url is null:
            raise ArgumentException("html did not contain openid.server link")

        self.identity_url = uri
        self.type_uris = (OPENID_1_0_TYPE,)
        self.used_yadis = false

    def UsesExtension(extension_uri as Uri):
        return extension_uri in self.type_uris


*/