# dotnet-api

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
  * No
* Authentication - Implicit grant and hybrid flows
  * All unchecked
* App roles
  * `user`
  * `admin`

## Development

### Adding database migrations

Ensure you have the `dotnet-ef` tool installed:

```
dotnet tool install --global dotnet-ef
```

Add a code first database migration:

```
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef migrations add "<migration_name>" --startup-project Host --project Repository --context DatabaseContext
```

### User secrets for development

```
# Database connection
dotnet user-secrets set "ConnectionStrings:DatabaseContext" "Server=localhost;Database=Api;Integrated Security=true;MultipleActiveResultSets=true" --project Host

# Authentication
dotnet user-secrets set "oidc:audience" "<audience>" --project Host
dotnet user-secrets set "oidc:authorityUri" "<authorityUri>" --project Host
dotnet user-secrets set "oidc:roles:user" "<user_role_id>" --project Host
dotnet user-secrets set "oidc:roles:admin" "<admin_role_id>" --project Host
dotnet user-secrets set "oidc:claim_types:name" "<name_claim_type>" --project Host
dotnet user-secrets set "oidc:claim_types:role" "<role_claim_type>" --project Host

# Message Queue using AMQP
dotnet user-secrets set "amqp:uri" "failover:amqp://<address>" --project Host
dotnet user-secrets set "amqp:username" "<username>" --project Host
dotnet user-secrets set "amqp:password" "<password>" --project Host
``` 