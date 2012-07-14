using System.Collections.Generic;
using System.Xml.XPath;

namespace Lpfm.LastFmScrobbler.Api
{
    /// <summary>
    /// An API for some of the methods of the Last.fm Auth (Authentication) API
    /// </summary>
    /// <remarks>This class is named for the API that is wraps</remarks>
    internal class AuthApi : IAuthApi
    {
        private const string AuthGetTokenXPath = "/lfm/token";
        private const string GetTokenMethodName = "auth.getToken";
        internal const string GetSessionMethodName = "auth.getSession";

        internal AuthApi() : this(new WebRequestRestApi())
        {
        }

        internal AuthApi(IRestApi restApi)
        {
            RestApi = restApi;
        }

        IRestApi RestApi { get; set; }

        #region IAuthApi Members

        /// <summary>
        /// Fetch an unathorized request token for an API account
        /// </summary>
        public AuthenticationToken GetToken(Authentication authentication)
        {
            var parameters = new Dictionary<string, string>();
            ApiHelper.AddRequiredParams(parameters, GetTokenMethodName, authentication, false);

            var navigator = RestApi.SendGetRequest(ApiHelper.LastFmWebServiceRootUrl, parameters);
            ApiHelper.CheckLastFmStatus(navigator);

            return new AuthenticationToken(ApiHelper.SelectSingleNode(navigator, AuthGetTokenXPath).Value);
        }

        /// <summary>
        /// Fetch a session key for a user
        /// </summary>
        public void GetSession(Authentication authentication, AuthenticationToken token)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("token", token.Value);

            ApiHelper.AddRequiredParams(parameters, GetSessionMethodName, authentication);

            XPathNavigator navigator = RestApi.SendGetRequest(ApiHelper.LastFmWebServiceRootUrl, parameters);
            ApiHelper.CheckLastFmStatus(navigator);

            authentication.Session = GetSessionFromNavigator(navigator);
        }

        #endregion

        protected virtual Session GetSessionFromNavigator(XPathNavigator navigator)
        {
            var session = new Session
                              {
                                  IsSubscriber = ApiHelper.SelectSingleNode(navigator, "lfm/session/subscriber").ValueAsBoolean,
                                  Key = ApiHelper.SelectSingleNode(navigator, "lfm/session/key").Value,
                                  Username = ApiHelper.SelectSingleNode(navigator, "lfm/session/name").Value
                              };
            return session;
        }
    }
}