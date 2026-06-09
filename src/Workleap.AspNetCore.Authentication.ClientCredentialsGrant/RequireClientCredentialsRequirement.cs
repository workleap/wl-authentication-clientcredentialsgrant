using Microsoft.AspNetCore.Authorization;

namespace Workleap.AspNetCore.Authentication.ClientCredentialsGrant;

/// <summary>
/// Public so that consumers can register their own <see cref="AuthorizationHandler{TRequirement}"/> for this
/// requirement and satisfy it under custom conditions. ASP.NET Core combines handlers with OR semantics, so a
/// consumer handler that calls <c>context.Succeed(requirement)</c> runs alongside the built-in scope check.
/// Consumer handlers must only ever call <c>Succeed</c> (never <c>Fail</c>), as a failure would override every
/// successful handler and block the built-in authorization path.
/// </summary>
public sealed class RequireClientCredentialsRequirement : IAuthorizationRequirement;