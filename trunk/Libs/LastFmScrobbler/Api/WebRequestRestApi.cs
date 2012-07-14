using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace Lpfm.LastFmScrobbler.Api
{
    /// <summary>
    /// An implementation of <see cref="IRestApi"/> based on <see cref="HttpWebRequest"/>
    /// </summary>
    internal class WebRequestRestApi : IRestApi
    {
        /// <summary>
        /// The Name Value Pair Format String used by this object
        /// </summary>
        public const string NameValuePairStringFormat = "{0}={1}";

        /// <summary>
        /// The Name-value pair seperator used by this object
        /// </summary>
        public const string NameValuePairStringSeperator = "&";

        private WebProxy _proxy = null;

        #region IRestApi Members

        /// <summary>
        /// Sends a GET request to the REST service
        /// </summary>
        /// <param name="url">A fully qualified URL</param>
        /// <param name="queryItems">A Dictionary of request query items</param>
        /// <returns>A read-only XPath queryable <see cref="XPathNavigator"/></returns>
        public XPathNavigator SendGetRequest(string url, Dictionary<string, string> queryItems)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            if (queryItems == null) throw new ArgumentNullException("queryItems");

            var builder = new UriBuilder(url);
            builder.Query = BuildStringOfItems(queryItems);

            ServicePointManager.Expect100Continue = false;
            var request = CreateWebRequest(builder.Uri);
            request.Method = "GET";

            return GetResponseAsXml(request);
        }


        /// <summary>
        /// Synchronously sends a POST request to the REST service and returns the XML Response
        /// </summary>
        /// <param name="url">A fully qualified URL</param>
        /// <param name="formItems">A <see cref="NameValueCollection"/> of name-value pairs to post in the body of the request</param>
        /// <returns>A read-only XPath queryable <see cref="XPathNavigator"/></returns>
        /// <remarks>Will synchronously HTTP POST a application/x-www-form-urlencoded request</remarks>
        public XPathNavigator SendPostRequest(string url, Dictionary<string, string> formItems)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            if (formItems == null) throw new ArgumentNullException("formItems");

            // http://msdn.microsoft.com/en-us/library/system.net.servicepointmanager.expect100continue.aspx
            ServicePointManager.Expect100Continue = false;
            var request = CreateWebRequest(url);
            request.Method = "POST";

            string postData = BuildStringOfItems(formItems);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            return GetResponseAsXml(request);
        }

        protected internal virtual XPathNavigator GetResponseAsXml(WebRequest request)
        {
            WebResponse response;
            XPathNavigator navigator;
            try
            {
                response = request.GetResponse();
                navigator = GetXpathDocumentFromResponse(response);
                ApiHelper.CheckLastFmStatus(navigator);
            }
            catch (WebException exception)
            {
                response = exception.Response;
                
                XPathNavigator document;
                TryGetXpathDocumentFromResponse(response, out document);

                if (document != null) ApiHelper.CheckLastFmStatus(document, exception);
                throw; // throw even if Last.fm status is OK
            }

            return navigator;
        }

        #endregion

        protected virtual string BuildStringOfItems(Dictionary<string, string> queryItems)
        {
            var builder = new StringBuilder();

            int count = 0;
            foreach (KeyValuePair<string, string> nameValue in queryItems)
            {
                if (count > 0) builder.Append(NameValuePairStringSeperator);
                builder.AppendFormat(NameValuePairStringFormat, nameValue.Key, HttpUtility.UrlEncode(nameValue.Value));
                count++;
            }

            return builder.ToString();
        }

        protected virtual bool TryGetXpathDocumentFromResponse(WebResponse response, out XPathNavigator document)
        {
            bool parsed;

            try
            {
                document = GetXpathDocumentFromResponse(response);
                parsed = true;
            }
            catch (Exception)
            {
                document = null;
                parsed = false;
            }

            return parsed;
        }

        protected virtual XPathNavigator GetXpathDocumentFromResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) throw new InvalidOperationException("Response Stream is null");

                try
                {
                    return new XPathDocument(stream).CreateNavigator();
                }
                catch (XmlException exception)
                {
                    throw new XmlException("Could not read HTTP Response as XML", exception);
                }
                finally
                {
                    response.Close();
                }
            }
        }

        protected virtual WebRequest CreateWebRequest(Uri uri)
        {
            var request = WebRequest.Create(uri);
            if (_proxy != null)
                request.Proxy = _proxy;

            return request;
        }

        protected virtual WebRequest CreateWebRequest(string uri)
        {
            return CreateWebRequest(new Uri(uri));
        }

    }
}