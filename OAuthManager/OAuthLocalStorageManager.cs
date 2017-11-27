using DatabaseDealer;
using TTOAuthManager.Azure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTOAuthManager
{
    internal class OAuthLocalStorageManager
    {
        private DatabaseManager _dbManager;
        private AuthConfig _authConfig;
        public OAuthLocalStorageManager(AuthConfig config, DatabaseManager dbmanager)
        {
            _dbManager = dbmanager;
            _authConfig = config;
        }

        public AccessTokenClass GetAccessToken(string stateId)
        {
            AccessTokenClass accessTokenClass = default(AccessTokenClass);
            AccessTokenClass[] accessTokens = _dbManager.ExecuteSQLReader<AccessTokenClass>("select * from access_tokens where stateid like '" + stateId + "'", x => new AccessTokenClass() { AccessToken = x["access_token"].ToString(), RefreshToken = x["refresh_token"].ToString(), ExtExpiresIn = Convert.ToInt32(x["ext_expires_in"]), ExpiresIn = Convert.ToInt32(x["expires_in"]), Scope = x["scope"].ToString(), TokenType = x["token_type"].ToString(), MnfDateTime = DateTime.Parse(x["MnfDateTime"].ToString()) });
            if (accessTokens.Length > 0)
            {
                accessTokenClass = accessTokens.FirstOrDefault();
                if (accessTokenClass.AccessToken == "")
                {
                    _dbManager.ExecuteSQLWriter("delete from access_tokens where stateid like '" + stateId + "'");
                    accessTokenClass = null;
                }
            }

            return accessTokenClass;
        }

        private string RetrieveAuthCode(string stateId)
        {
            // check in the database if it exist
            var query = "select * from AuthenticationCode where stateid like '" + stateId + "'";
            object[] result = _dbManager.ExecuteSQLReader(query, row => row["code"]);
            if (result.Length == 0)
            {
                return "Not Authorized";
            }
            else
            {
                return result[0].ToString();
            }
        }

        public AuthCode GetAuthCode(string _stateId)
        {
            AuthCode authCode = null;

            AuthCode[] authCodes = _dbManager.ExecuteSQLReader<AuthCode>("select * from AuthenticationCode where stateid like '" + _stateId + "'", x => new AuthCode() { code = x["code"].ToString(), session_state = x["session_state"].ToString(), stateid = x["stateid"].ToString() });
            if (authCodes.Length > 0)
            {
                authCode = authCodes.FirstOrDefault();
            }

            return authCode;

        }

        internal void AddAccessToken(string stateId, AccessTokenClass token)
        {
            var query = string.Format("insert into access_tokens values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", stateId, token.AccessToken, token.RefreshToken, token.TokenType, _authConfig.Scope, token.ExtExpiresIn, token.ExpiresIn, DateTime.UtcNow.ToString());
            _dbManager.ExecuteSQLWriter(query);
        }

        internal void UpdateAccessToken(string stateId, AccessTokenClass token)
        {
            var query = string.Format("update access_tokens set access_token='{0}',refresh_token='{1}',ext_expires_in='{2}',expires_in='{3}',MnfDateTime='{4}' where stateid='{5}'", token.AccessToken, token.RefreshToken, token.ExtExpiresIn, token.ExpiresIn, DateTime.UtcNow.ToString(), stateId);
            _dbManager.ExecuteSQLWriter(query);
        }
    }
}
