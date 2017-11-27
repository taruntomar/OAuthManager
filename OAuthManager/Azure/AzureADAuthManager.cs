using Newtonsoft.Json;
using TTOAuthManager.Azure.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTOAuthManager
{
    class AzureADAuthManager
    {
        private AuthConfig _authConfig;
        private string _authEndPoint;
        private string _tokenEndPoint;
        public AzureADAuthManager(AuthConfig config)
        {
            _authConfig = new AuthConfig();
            _authEndPoint = string.Format("{0}/{1}/{2}/authorize", _authConfig.baseUrl,_authConfig.TanentId, _authConfig.OAuthVersion);
            _tokenEndPoint = string.Format("{0}/{1}/{2}/token", _authConfig.baseUrl, _authConfig.TanentId, _authConfig.OAuthVersion);

        }

        public AzureADAuthRestResponse<AccessTokenClass, OAuthError> GetAccessToken(string code, TokenRetrivalType tokenRetrivalType)
        {
            RestClient client = new RestClient(_tokenEndPoint);
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("client_id", _authConfig.ClientId);
            request.AddParameter("scope", _authConfig.Scope);

            if (tokenRetrivalType == TokenRetrivalType.AuthorizationCode)
            {
                request.AddParameter("code", code);
                request.AddParameter("grant_type", "authorization_code");
            }
            else if (tokenRetrivalType == TokenRetrivalType.RefreshToken)
            {
                request.AddParameter("refresh_token", code);
                request.AddParameter("grant_type", "refresh_token");
            }

            request.AddParameter("redirect_uri", _authConfig.RedirectURL);

            request.AddParameter("client_secret",_authConfig.ClientSecret);

            IRestResponse response = client.Execute(request);
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
