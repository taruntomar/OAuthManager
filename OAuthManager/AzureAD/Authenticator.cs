using Newtonsoft.Json;
using Open.OAuthManager.Azure.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Open.OAuthManager.AzureAD
{
    public class Authenticator
    {
        private AuthConfig _authConfig;
        private string _authEndPoint;
        private string _tokenEndPoint;
        private RestClient _client;
        private RestRequest _request;

        public Authenticator(AuthConfig config)
        {
            _authConfig = new AuthConfig();
            _authEndPoint = string.Format("{0}/{1}/{2}/authorize", _authConfig.baseUrl,_authConfig.TanentId, _authConfig.OAuthVersion);
            _tokenEndPoint = string.Format("{0}/{1}/{2}/token", _authConfig.baseUrl, _authConfig.TanentId, _authConfig.OAuthVersion);

        }

        public AzureADAuthRestResponse<AccessTokenClass, OAuthError> GetAccessToken(string code, TokenRetrivalType tokenRetrivalType)
        {
            _client = new RestClient(_tokenEndPoint);
            _request = new RestRequest(Method.POST);
            _request.AddParameter("client_id", _authConfig.ClientId);
            _request.AddParameter("scope", _authConfig.Scope);

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

            _request.AddParameter("redirect_uri", _authConfig.RedirectURL);

            _request.AddParameter("client_secret",_authConfig.ClientSecret);

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
            _client.BaseUrl = new Uri(_tokenEndPoint);
            _request.Parameters.Clear();
            _request.Method = Method.POST;
            _request.AddParameter("client_id", _authConfig.ClientId);
            _request.AddParameter("grant_type", "client_credentials");
            _request.AddParameter("resource", _authConfig.Resource);
            _request.AddParameter("client_secret", _authConfig.ClientSecret);
            _request.AddParameter("scope", _authConfig.Scope);

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
