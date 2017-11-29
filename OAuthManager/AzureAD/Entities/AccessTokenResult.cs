namespace Open.OAuthManager.Azure.Entities
{
    public class AccessTokenResult
    {
        public OAuthErrors oAuthErrors { get; set; }
        public string AccessToken { get; set; }

    }
}
