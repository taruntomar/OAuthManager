using DatabaseDealer;
using Open.OAuthManager.Azure.Entities;
using System;
using System.Linq;


namespace Open.OAuthManager
{
    ///<summary>
    /// This class provide services to store and retrieve
    /// user information in database. 
    /// This class is dependent on AuthConfig and DatabaseManager classes.
    /// To create instance of this class, user has to provide instance of AuthConfig and DatabaseManager.
    /// <code>
    /// AuthConfig config = new AuthConfig();
    /// DatabaseManager dbManager = new DatabaseManager("{connection-string}");
    /// LocalStorageManager localStoreManager = new LocalStoreManager(config,dbManager);
    /// 
    /// </code>
    ///</summary>
    public class LocalStorageManager
    {
        private DatabaseManager _dbManager;
        private AuthConfig _authConfig;
        public LocalStorageManager(AuthConfig config, DatabaseManager dbmanager)
        {
            _dbManager = dbmanager;
            _authConfig = config;
        }
        ///<summary>
        /// This method search for the AccessToken in database based on stateId.
        /// An stateId is the unique code generated for combination of a user and a particular scope.
        /// AccessTokens are stored in database with stateId as primary key.
        /// If no AccessToken available in database then it returns null.
        /// <code>
        /// AccessTokenClass accessToken= localStoreManager.GetAccessToken(stateId);
        /// </code>
        ///</summary>
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
      
        ///<summary>
        /// This method search for the AuthCode in database based on stateId.
        /// An stateId is the unique code generated for combination of a user and a particular scope.
        /// AuthCodes are stored in database with stateId as primary key.
        /// If no AuthCode available in database then it returns null.
        /// <code>
        /// string authCode= localStoreManager.GetAuthCode(stateId);
        /// </code>
        ///</summary>
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
        ///<summary>
        /// This method add AccessToken in database by associating with an stateId.
        /// An stateId is the unique code generated for combination of a user and a particular scope.
        /// <code>
        /// localStoreManager.AddAccessToken(stateId,accessToken);
        /// </code>
        ///</summary>
        public void AddAccessToken(string stateId, AccessTokenClass accessTokenClass)
        {
            var query = string.Format("insert into access_tokens values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", stateId, accessTokenClass.AccessToken, accessTokenClass.RefreshToken, accessTokenClass.TokenType, _authConfig.Scope, accessTokenClass.ExtExpiresIn, accessTokenClass.ExpiresIn, DateTime.UtcNow.ToString());
            _dbManager.ExecuteSQLWriter(query);
        }
        ///<summary>
        /// This method update an already existing AccessToken in databasem, associated with given stateId.
        /// An stateId is the unique code generated for combination of a user and a particular scope.
        /// <code>
        /// localStoreManager.UpdateAccessToken(stateId,accessTokenClass);
        /// </code>
        ///</summary>
        public void UpdateAccessToken(string stateId, AccessTokenClass accessTokenClass)
        {
            var query = string.Format("update access_tokens set access_token='{0}',refresh_token='{1}',ext_expires_in='{2}',expires_in='{3}',MnfDateTime='{4}' where stateid='{5}'", accessTokenClass.AccessToken, accessTokenClass.RefreshToken, accessTokenClass.ExtExpiresIn, accessTokenClass.ExpiresIn, DateTime.UtcNow.ToString(), stateId);
            _dbManager.ExecuteSQLWriter(query);
        }
    }
}
