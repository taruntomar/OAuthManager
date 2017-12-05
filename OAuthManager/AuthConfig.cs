using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open.OAuthManager
{
    ///<summary>
    /// Configuration Class - It store all config details
    /// <code>
    /// AuthConfig config = new AuthConfig();
    /// config.LoggedInUserEmail = "exampleuser@host.com"
    /// 
    /// </code>
    ///</summary>
    public class AuthConfig
    {
        public string LoggedInUserEmail { get; set; }
        public string Scope { get; set; }
        public string TanentId { get; set; }
        public string Authority { get; set; }
        public string OAuthVersion { get; set; }
        public string RedirectURL { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Resource { get; set; }


    }
}
