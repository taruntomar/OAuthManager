using DatabaseDealer;
using Open.OAuthManager.Azure.Entities;
using RestSharp;


namespace Open.OAuthManager.AzureAD
{
    public class AuthManager
    {
        private string _stateId;
        public AuthConfig Config { get; set; }

        private StateManager _stateIdManager;
        private LocalStorageManager _localStorage;
        private Authenticator _authenticator;

        public AuthManager(AuthConfig config, DatabaseManager dbmanager)
        {
            Config = config;
            Config.OAuthVersion = "oauth2/v2.0";

            _stateIdManager = new StateManager(dbmanager,Config.LoggedInUserEmail,Config.Scope);
            _stateId = _stateIdManager.GetStateIdForCurrentUser();

            _localStorage = new LocalStorageManager(Config,dbmanager);
            _authenticator = new Authenticator(Config);

        }
        public AzureADAuthRestResponse<AccessTokenClass, OAuthErrors> GetAccessToken()
        {
            var resp = new AzureADAuthRestResponse<AccessTokenClass, OAuthErrors>();
            // try to look for the available token first
            // if token not available then use authorize code to download it
            // if token expires then download new token using refresh token and store in database
            // if authorize token not available send send the response then auhorize token not available
            IRestResponse respose;
            AccessTokenClass token = _localStorage.GetAccessToken(_stateId);
            if (token == null)
            {
                // check if authcode available
                AuthCode authcode = _localStorage.GetAuthCode(_stateId);
                if (authcode == null)
                {
                    resp.Error = OAuthErrors.AuthCodeNotFound;
                    return resp;

                }
                var res = _authenticator.GetAccessToken(authcode.code, TokenRetrivalType.AuthorizationCode);
                // check for any error in response
                if (res.IsSucceeded)
                {
                    token = res.Result;
                    _localStorage.AddAccessToken(_stateId,token);
                    // add token into database
                    
                }
                else
                {
                    // send status to front page that it is not authorized, authorization code has expired
                    resp.Error = OAuthErrors.AuthCodeExpires;
                    return resp;

                }

            }
            // check for token's validity, if expires then recalculate from refresh token
            if (!_authenticator.validateToken(token))
            {
                var res = _authenticator.GetAccessToken(token.RefreshToken, TokenRetrivalType.RefreshToken);
                // check for any error, if error is not present then cast to 
                if (res.IsSucceeded)
                {
                    token = res.Result;
                    _localStorage.UpdateAccessToken(_stateId, token);
                    
                }
            }
            resp.Result = token;
            resp.Error = OAuthErrors.None;
            return resp;

        }
    }
}
