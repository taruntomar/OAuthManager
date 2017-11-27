using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthManager.Azure.Entities
{
    public class AzureADAuthRestResponse<R, E>
    {
        public R Result { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OAuthErrors OAuthError { get; set; }

        public E Error { get; set; }

        public bool IsSucceeded { get; set; }

    }
}
