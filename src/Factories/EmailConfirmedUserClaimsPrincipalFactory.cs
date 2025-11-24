using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace controller_api_test.src.Factories;

public class EmailConfirmedUserClaimsPrincipalFactory(
    UserManager<IdentityUser> userManager,
    IOptions<IdentityOptions> optionsAccessor) : UserClaimsPrincipalFactory<IdentityUser>(userManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        // This MUST match the claim type used in your policy ("email_confirmed")
        identity.AddClaim(new Claim("email_confirmed", user.EmailConfirmed.ToString().ToLowerInvariant()));
        return identity;
    }
}
