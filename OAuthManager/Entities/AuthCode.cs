using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTOAuthManager
{
    public class AuthCode
    {
        public string code { get; set; }
        public string stateid { get; set; }
        public string session_state { get; set; }
    }
}
