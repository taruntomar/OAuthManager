# OAuthManager <a href="https://travis-ci.org/taruntomar/OAuthManager" target="_blank"><img src="https://travis-ci.org/taruntomar/OAuthManager.svg?branch=master" /></a>
A library to facilitate authentication using OAuth protocol

Firstly create the configuration object

# Quick Start Guide
```c#
AuthConfig config = new AuthConfig();
config.LoggedInUserEmail = "<current_user_email>";
config.TanentId = "<TanentID_in_AzureAD>";
config.RedirectURL = "<URL_which_will_call_to_pass_authcode>";
config.ClientId="<ClientID_RegisteredWithAzureAD>";

DatabaseManager dbManager = new DatabaseManager(connectionString);
AzureADE2EManager authManager = new AzureADE2EManager(config,dbManager);
Token token = authManager.GetAccessToken();
```

