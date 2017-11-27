using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTOAuthManager.Azure.Entities
{
    public partial class OAuthError
    {
        [JsonProperty("correlation_id")]
        public string CorrelationId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_codes")]
        public long[] ErrorCodes { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("trace_id")]
        public string TraceId { get; set; }
    }
}
