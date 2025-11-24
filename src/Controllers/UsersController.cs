using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Asp.Versioning;
using controller_api_test.src.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace controller_api_test.src.Controllers;


[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
public class UsersController(UserManager<IdentityUser> userManager, IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory, IConfiguration config) : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<IdentityUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IConfiguration _config = config;

    [HttpPost(Name = "CreateUser")]
    public async Task<ActionResult> CreateUser(CreateUserDto createUserDto)
    {
        var user = new IdentityUser
        {
            UserName = createUserDto.Email,
            Email = createUserDto.Email
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Assign default permission claim
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("permission", "movies:read"));

        // Generate email confirmation token (activation)
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Return token to client (or send via email)
        return Ok(new { user.Id, user.Email, ActivationToken = token });
    }


    [HttpPut("activated", Name = "ActivateUser")]
    public async Task<ActionResult> ActivateUser(ActivateUserDto activateUserDto)
    {
        var user = await _userManager.FindByEmailAsync(activateUserDto.Email);
        if (user == null) return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, activateUserDto.Token);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { user.Id, user.Email, Activated = true });
    }

    [HttpPost("login", Name = "LoginUser")]
    public async Task<ActionResult> LoginUser(LoginUserDto loginUserDto)
    {
        var user = await _userManager.FindByEmailAsync(loginUserDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginUserDto.Password))
            return Unauthorized();

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var claims = principal.Claims;

        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"]!;
        var jwtAudience = _config["Jwt:Audience"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            audience: jwtAudience,
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}


