using DatabaseDealer;
using Open.OAuthManager.Azure.Entities;
using RestSharp;


namespace Open.OAuthManager.AzureAD
{
    /*
     * Authenticator V2 is an advance authenticator,
     * which also have DB implementation
     */
    public class AuthenticatorV2:Authenticator
    {
        
        public AzureADAuthRestResponse<AccessTokenClass, OAuthErrors> GetAccessToken_UserCredential(DatabaseManager dbmanager)
        {
            StateManager _stateIdManager = new StateManager(Config.LoggedInUserEmail, Config.Scope);
            LocalStorageManager _localStorage = new LocalStorageManager(Config, dbmanager);
            string _stateId = _stateIdManager.GetStateIdForCurrentUser(dbmanager);
            var resp = new AzureADAuthRestResponse<AccessTokenClass, OAuthErrors>();
            // try to look for the available token first
            // if token not available then use authorize code to download it
            // if token expires then download new token using refresh token and store in database
            // if authorize token not available send send the response then auhorize token not available
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
                var res = GetAccessToken(authcode.code, TokenRetrivalType.AuthorizationCode);
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
            if (!validateToken(token))
            {
                var res = GetAccessToken(token.RefreshToken, TokenRetrivalType.RefreshToken);
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
