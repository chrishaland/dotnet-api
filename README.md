# dotnet-api

[![Build Status](https://chrishaland.visualstudio.com/dotnet-api/_apis/build/status/Build%20pipeline?branchName=main)](https://chrishaland.visualstudio.com/dotnet-api/_build/latest?definitionId=5&branchName=main)

## Configuration

### Configuring OpenId Connect Client
The following configuration describes creating a new OpenID Connect client in [Azure Active Directory](https://portal.azure.com/).

Go to the [Azure Portal](https://portal.azure.com/) and open the resource `Azure Active Directory`. 

Go to `App registrations` and register a new application:

* Single tenant
* Redirect URI as `Single-page application` (multiple values can be added later in the clients `Authentication` section)
  * http(s)://\<hostname>/authentication/callback
  * http(s)://\<hostname>/authentication/silent_callback

Once created, make sure the following settings are updated / correct:

* Authentication - Allow public client flows
  * Yes
* Authentication - Implicit grant and hybrid flows
  * All unchecked
* Token configuration
  * Add group claim, with value `Group ID` and `Emit groups as role claims` as checked
    * Alternativly, use app roles

## Development

### Adding database migrations

```
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef migrations add "<migration_name>" --startup-project Host --project Repository --context DatabaseContext
```

### User secrets for development

```
# Database connection
dotnet user-secrets set "ConnectionStrings:Database" "Server=localhost;Database=Api;Integrated Security=true;MultipleActiveResultSets=true" --project Host

# Authentication
dotnet user-secrets set "oidc:audience" "<audience>" --project Host
dotnet user-secrets set "oidc:authorityUri" "<authorityUri>" --project Host
dotnet user-secrets set "oidc:roles:user" "<user_role_id>" --project Host
dotnet user-secrets set "oidc:roles:admin" "<admin_role_id>" --project Host
dotnet user-secrets set "oidc:claim_types:name" "<name_claim_type>" --project Host
dotnet user-secrets set "oidc:claim_types:role" "<role_claim_type>" --project Host
```