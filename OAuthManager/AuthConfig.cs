using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTOAuthManager
{
    public class AuthConfig
    {
        public string LoggedInUserEmail { get; set; }
        public string Scope { get; set; }
        public string TanentId { get; set; }
        public string baseUrl { get; set; }
        public string OAuthVersion { get; set; }
        public string RedirectURL { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

    }
}
