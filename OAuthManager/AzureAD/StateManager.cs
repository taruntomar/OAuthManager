using DatabaseDealer;
using System;

namespace Open.OAuthManager.AzureAD
{
    class StateManager
    {
        private string _loggedInUserMail;
        private string _scope;
        public StateManager(string loggedInUserMail, string scope)
        {
            _loggedInUserMail = loggedInUserMail;
            _scope = scope;
        }

        public string NewStateId()
        {
            return Guid.NewGuid().ToString();
        }
        public string GetStateIdForCurrentUser(DatabaseManager dbmanager)
        {
            // get current logged in name
            string stateid;

            var query = "select * from UserAuthorizationScopeIds where email like '" + _loggedInUserMail + "'";
            object[] result = dbmanager.ExecuteSQLReader(query, row => row["stateid"]);
            if (result.Length > 0)
            {
                stateid = result[0].ToString();
                // update scopes 
                dbmanager.ExecuteSQLWriter("update UserAuthorizationScopeIds set scope='" + _scope + "' where email like '" + _loggedInUserMail + "'");
            }
            else
            {
                stateid = NewStateId();
                dbmanager.ExecuteSQLWriter("insert into UserAuthorizationScopeIds values('" + _loggedInUserMail + "','" + _scope + "','" + stateid + "')");
            }
            return stateid;
        }
    }
}
