using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.XPath;

namespace Lpfm.LastFmScrobbler.Api
{
    internal class ApiHelper
    {
        const string LastFmStatusOk = "ok";
        const string LastFmErrorXPath = "/lfm/error";
        const string LastFmErrorCodeXPath = "/lfm/error/@code";
        const string LastFmStatusXPath = "/lfm/@status";
        
        /// <summary>
        /// The Last.fm web service root URL
        /// </summary>
        public const string LastFmWebServiceRootUrl = "http://ws.audioscrobbler.com/2.0/";

        public const string MethodParamName = "method";
        public const string ApiKeyParamName = "api_key";
        public const string ApiSignatureParamName = "api_sig";
        public const string SessionKeyParamName = "sk";

        /// <summary>
        /// Check the Last.fm status of the response and throw a <see cref="LastFmApiException"/> if an error is detected
        /// </summary>
        /// <param name="navigator">The response as <see cref="XPathNavigator"/></param>
        /// <exception cref="LastFmApiException"/>
        public static void CheckLastFmStatus(XPathNavigator navigator)
        {
            CheckLastFmStatus(navigator, null);
        }

        /// <summary>
        /// Check the Last.fm status of the response and throw a <see cref="LastFmApiException"/> if an error is detected
        /// </summary>
        /// <param name="navigator">The response as <see cref="XPathNavigator"/></param>
        /// <param name="webException">An optional <see cref="WebException"/> to be set as the inner exception</param>
        /// <exception cref="LastFmApiException"/>
        public static void CheckLastFmStatus(XPathNavigator navigator, WebException webException)
        {
            var node = SelectSingleNode(navigator, LastFmStatusXPath);

            if (node.Value == LastFmStatusOk) return;

            throw new LastFmApiException(string.Format("LastFm status = \"{0}\". Error code = {1}. {2}",
                                                       node.Value,
                                                       SelectSingleNode(navigator, LastFmErrorCodeXPath),
                                                       SelectSingleNode(navigator, LastFmErrorXPath)), webException);
        }

        /// <summary>
        /// Helper method to select a single node from an <see cref="XPathNavigator"/> as <see cref="XPathNavigator"/>
        /// </summary>
        public static XPathNavigator SelectSingleNode(XPathNavigator navigator, string xpath)
        {
            var node = navigator.SelectSingleNode(xpath);
            if (node == null) throw new InvalidOperationException("Node is null. Cannot select single node. XML response may be mal-formed");
            return node;
        }

        /// <summary>
        /// Adds the parameters that are required by the Last.Fm API to the <see cref="parameters"/> dictionary
        /// </summary>
        public static void AddRequiredParams(Dictionary<string, string> parameters, string methodName, Authentication authentication, bool addApiSignature = true)
        {
            // method
            parameters.Add(MethodParamName, methodName);

            // api key
            parameters.Add(ApiKeyParamName, authentication.ApiKey);

            // session key
            if (authentication.Session != null) parameters.Add(SessionKeyParamName, authentication.Session.Key);

            // api_sig
            if (addApiSignature)
            {
                parameters.Add(ApiSignatureParamName, GetApiSignature(parameters, authentication.ApiSecret));
            }
        }

        /// <summary>
        /// Generates a hashed Last.fm API Signature from the parameter name-value pairs, and the API secret
        /// </summary>
        public static string GetApiSignature(Dictionary<string, string> nameValues, string apiSecret)
        {
            string parameters = GetStringOfOrderedParamsForHashing(nameValues);
            parameters += apiSecret;

            return Hash(parameters);
        }

        /// <summary>
        /// Gets a string of ordered parameter values for hashing
        /// </summary>
        public static string GetStringOfOrderedParamsForHashing(Dictionary<string, string> nameValues)
        {
            var paramsBuilder = new StringBuilder();

            foreach (KeyValuePair<string, string> nameValue in nameValues.OrderBy(nv => nv.Key))
            {
                paramsBuilder.Append(string.Format("{0}{1}", nameValue.Key, nameValue.Value));
            }
            return paramsBuilder.ToString();
        }

        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx
        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        public static string Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

    }
}
