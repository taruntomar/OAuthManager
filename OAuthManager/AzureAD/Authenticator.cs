using Newtonsoft.Json;
using Open.OAuthManager.Azure.Entities;
using RestSharp;
using System;
using Newtonsoft.Json.Linq;

namespace Open.OAuthManager.AzureAD
{
    public enum EndPointType
    {
        authorize,
        token
    }
    public class Authenticator
    {
        public AuthConfig Config { get; set; }
        
        private RestRequest _request;

        public Authenticator()
        {
            Config = new AuthConfig();
            Config.OAuthVersion = "oauth2/v2.0";
        }
        
        private string  GetEndPoint(EndPointType endPointType)
        {
            string endpointype = endPointType == EndPointType.authorize ? "authorize" : "token";
            return string.Format("{0}/{1}/{2}/{3}", Config.baseUrl, Config.TanentId, Config.OAuthVersion, endpointype); 
        }

        public string GetAuthorizationCodeUrl(string baseUrl,string tanentId,string clientId,string scope,string response_type,string redirect_uri,string response_mode)
        {
            Config.baseUrl = baseUrl;
            Config.TanentId = tanentId;
            Config.Scope = scope;
            string stateId = new StateManager(Config.LoggedInUserEmail,Config.Scope).NewStateId();
            var url = GetEndPoint(EndPointType.authorize)
                + "?client_id=" + clientId
                + "&response_type=" + response_type
                + "&redirect_uri=" + redirect_uri
                + "&response_mode=" + response_mode
                + "&scope=" + Config.Scope
                + "&state=" + stateId;
            return url;
        }

        public AzureADAuthRestResponse<AccessTokenClass, OAuthError> GetAccessToken(string code, TokenRetrivalType tokenRetrivalType)
        {
            RestClient  _client = new RestClient(GetEndPoint(EndPointType.authorize));
            _request = new RestRequest(Method.POST);
            _request.AddParameter("client_id", Config.ClientId);
            _request.AddParameter("scope", Config.Scope);

            if (tokenRetrivalType == TokenRetrivalType.AuthorizationCode)
            {
                _request.AddParameter("code", code);
                _request.AddParameter("grant_type", "authorization_code");
            }
            else if (tokenRetrivalType == TokenRetrivalType.RefreshToken)
            {
                _request.AddParameter("refresh_token", code);
                _request.AddParameter("grant_type", "refresh_token");
            }

            _request.AddParameter("redirect_uri", Config.RedirectURL);

            _request.AddParameter("client_secret",Config.ClientSecret);

            IRestResponse response = _client.Execute(_request);
            AzureADAuthRestResponse<AccessTokenClass, OAuthError> resp = new AzureADAuthRestResponse<AccessTokenClass, OAuthError>();
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                OAuthError error = JsonConvert.DeserializeObject<OAuthError>(response.Content, Converter.Settings);
                resp.Error = error;
                resp.IsSucceeded = false;
            }
            else
            {
                AccessTokenClass result = JsonConvert.DeserializeObject<AccessTokenClass>(response.Content, Converter.Settings);
                resp.Result = result;
                resp.IsSucceeded = true;
            }
            return resp;
        }

        public string GetAccessToken_FromClientCredential()
        {
            RestClient _client = new RestClient();
            _client.BaseUrl = new Uri(GetEndPoint(EndPointType.token));
            _request.Parameters.Clear();
            _request.Method = Method.POST;
            _request.AddParameter("client_id", Config.ClientId);
            _request.AddParameter("grant_type", "client_credentials");
            _request.AddParameter("resource", Config.Resource);
            _request.AddParameter("client_secret", Config.ClientSecret);
            _request.AddParameter("scope", Config.Scope);

            var response = _client.Execute(_request);
            JObject obj = JObject.Parse(response.Content);

            string x = obj["access_token"].ToString();
            return x;

        }
        public void SetAuthorizationHeader(RestRequest request,string bearer)
        {
            request.AddHeader("authorization", "bearer " + bearer);
        }
        public bool validateToken(AccessTokenClass token)
        {
            // check the time when token was retrived, add the expire time, that time should be less than the current time
            bool valid = false;

            DateTime MnfDateTime = token.MnfDateTime;
            DateTime expireDateTime = MnfDateTime.AddSeconds(token.ExpiresIn);
            int diff = DateTime.Compare(expireDateTime, DateTime.UtcNow);
            if (diff > 0)
                valid = true;

            return valid;

        }
    }
}
