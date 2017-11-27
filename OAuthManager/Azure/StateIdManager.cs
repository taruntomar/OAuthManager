using DatabaseDealer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthManager.Azure
{
    class StateIdManager
    {
        private DatabaseManager _dbmanager;
        private string _loggedInUserMail;
        private string _scope;
        public StateIdManager(DatabaseManager dbmanager,string loggedInUserMail, string scope)
        {
            _dbmanager = dbmanager;
            _loggedInUserMail = loggedInUserMail;
            _scope = scope;
        }
        public string GetStateId()
        {
            // get current logged in name
            string stateid;

            var query = "select * from UserAuthorizationScopeIds where email like '" + _loggedInUserMail + "'";
            object[] result = _dbmanager.ExecuteSQLReader(query, row => row["stateid"]);
            if (result.Length > 0)
            {
                stateid = result[0].ToString();
                // update scopes 
                _dbmanager.ExecuteSQLWriter("update UserAuthorizationScopeIds set scope='" + _scope + "' where email like '" + _loggedInUserMail + "'");
            }
            else
            {
                stateid = Guid.NewGuid().ToString();
                _dbmanager.ExecuteSQLWriter("insert into UserAuthorizationScopeIds values('" + _loggedInUserMail + "','" + _scope + "','" + stateid + "')");
            }
            return stateid;
        }
    }
}
