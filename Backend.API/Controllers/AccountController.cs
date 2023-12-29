using Backend.API.Entities;
using Backend.API.Interfaces;
using Backend.API.Models;
using Backend.API.Permissions;
using Backend.API.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    IAccessControlService accessControl,
    IOptions<JwtSettings> jwtSettings,
    ILdapService ldapService,
    IMemoryCache memoryCache,
    ILogger<AccountController> logger,
    IOptions<LdapSetting> ldapSettings,
    UserManager<ApplicationUser> userManager)
    : ControllerBase
{
    private readonly LdapSetting _ldapSettings = ldapSettings.Value;
    private readonly int _refreshTokenExpiration = jwtSettings.Value.RefreshExpirationTime;

    [HttpPost("Refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseApiResponse<AuthenticationResponse>>> RefreshToken(
        [FromBody] RefreshTokenInput input)
    {
        var refreshToken = input.RefreshToken;
        var userEmail = await accessControl.RefreshTokenExists(refreshToken);

        if (string.IsNullOrWhiteSpace(userEmail)) return Unauthorized();

        if (memoryCache.TryGetValue(userEmail, out _))
        {
            await accessControl.SetUserRefreshToken(userEmail, "", TimeSpan.Zero);
            memoryCache.Remove(userEmail);
            return Unauthorized();
        }

        var claims = await accessControl.GetUserClaimsBy(userEmail);
        var jwtToken = accessControl.GenerateJwtToken(claims);

        var newRefreshToken = accessControl.GenerateRefreshToken();
        var updateSuccessfully = await accessControl.SetUserRefreshToken(userEmail, newRefreshToken,
            TimeSpan.FromMinutes(_refreshTokenExpiration));
        if (!updateSuccessfully)
            return Conflict(new BaseApiResponse<AuthenticationResponse>
                { Errors = ["Cannot update refresh token"] });

        return Ok(new BaseApiResponse<AuthenticationResponse>
        {
            Result = new AuthenticationResponse
            {
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken
            }
        });
    }

    [HttpPost("Login")]
    public async Task<ActionResult<BaseApiResponse<AuthenticationResponse>>> Login([FromBody] LoginInput credential)
    {
        var response = new BaseApiResponse<AuthenticationResponse>();
        if (!ModelState.IsValid)
        {
            response.AddModelErrors(ModelState);
            return BadRequest(response);
        }

        logger.LogTrace($"Login Request received for Email: {credential.Email}");
        var user = await accessControl.GetUserClaimsBy(credential.Email);
        if (user != null)
        {
            logger.LogTrace($"Email: {credential.Email} has access to login");
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var appUser = await userManager.FindByEmailAsync(credential.Email);
            var result = _ldapSettings.Enable
                ? await ldapService.Authenticate(credential.Email, credential.Password)
                : string.IsNullOrWhiteSpace(appUser?.PasswordHash) is false &&
                  passwordHasher.VerifyHashedPassword(appUser, appUser.PasswordHash, credential.Password) ==
                  PasswordVerificationResult.Success;
            if (result)
            {
                var token = accessControl.GenerateJwtToken(user);
                var refreshToken = accessControl.GenerateRefreshToken();
                var updateSuccessfully = await accessControl.SetUserRefreshToken(credential.Email, refreshToken,
                    TimeSpan.FromMinutes(_refreshTokenExpiration));
                if (!updateSuccessfully)
                    return Conflict(new BaseApiResponse<AuthenticationResponse>
                        { Errors = ["Cannot update refresh token"] });

                return Ok(new BaseApiResponse<AuthenticationResponse>
                {
                    Result = new AuthenticationResponse
                    {
                        JwtToken = token,
                        RefreshToken = refreshToken
                    }
                });
            }

            logger.LogError($"user with email: {credential.Email} has not access to LDAP");
            return Unauthorized();
        }

        logger.LogError($"there is no user with email: {credential.Email} in identity database");
        return NotFound();
    }

    [HttpPost("Register")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<string>>> Register([FromBody] UserRegisterInput input)
    {
        if (!ModelState.IsValid)
        {
            var response = new BaseApiResponse<string>();
            response.AddModelErrors(ModelState);
            return BadRequest(response);
        }

        var result = await accessControl.CreateUser(input);
        if (result.Any())
            return BadRequest(new BaseApiResponse<string> { Errors = result });
        return Ok(new BaseApiResponse<string>("OK"));
    }

    [HttpDelete("User/{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<string>>> DisableUser([FromRoute] string id)
    {
        var result = await accessControl.DisableUser(id);

        if (!result.Succeeded && result.UpdatedUser == null)
            return NotFound(new BaseApiResponse<string> { Errors = new List<string> { "User not found" } });
        memoryCache.Set(result.UpdatedUser.Email, 1);
        return Ok(new BaseApiResponse<string>("done"));
    }

    [HttpGet("User/{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<UserVM>>> GetUser([FromRoute] string id)
    {
        var result = await accessControl.GetUsersById(id);
        return Ok(new BaseApiResponse<UserVM> { Result = result });
    }

    [HttpGet("Permission")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public ActionResult<BaseApiResponse<PermissionList>> GetAllPermissions()
    {
        return Ok(new BaseApiResponse<PermissionList>(new PermissionList()));
    }

    [HttpPost("Role")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<List<string>>>> CreateRole([FromBody] RoleInput input)
    {
        var result = await accessControl.CreateRole(input);
        return Ok(new BaseApiResponse<List<string>>(result));
    }

    [HttpGet("Role")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<List<RoleItem>>>> GetAllRoles()
    {
        var result = await accessControl.GetAllRoles();
        return Ok(new BaseApiResponse<List<RoleItem>>(result));
    }

    [HttpPut("Role/{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<bool>>> UpdateRole([FromRoute] string id, [FromBody] RoleInput roleInput)
    {
        var result = await accessControl.UpdateRolePermissions(id, roleInput);
        var usersWithThisRole = await accessControl.GetUsersByRoleId(id);
        usersWithThisRole.ForEach(x => { memoryCache.Set(x.Email, 1); });
        return Ok(new BaseApiResponse<bool>(result));
    }

    [HttpGet("User")]
    [Authorize(Policy = PolicyTypes.Users.View)]
    public async Task<ActionResult<BaseApiResponse<UserListVM>>> GetAllUsers([FromQuery] UserFilterInput filterInput)
    {
        if (filterInput.RowCount == default) filterInput.RowCount = 15;
        if (filterInput.PageNumber == default) filterInput.PageNumber = 1;

        var result = await accessControl.GetUsers(filterInput);

        if (!result.Users.Any()) return NotFound();

        return Ok(new BaseApiResponse<UserListVM> { Result = result });
    }

    [HttpPut("User/{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    public async Task<ActionResult<BaseApiResponse<bool>>> UpdateUser([FromRoute] string id,
        [FromBody] UpdateUserInput updateInput)
    {
        var result = await accessControl.UpdateUser(id, updateInput);
        if (!result) return NotFound();

        return Ok(new BaseApiResponse<bool> { Result = result });
    }
}