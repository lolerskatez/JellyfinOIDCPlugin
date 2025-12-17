using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JellyfinOIDCPlugin.Controllers;

[ApiController]
[Route("api/oidc")]
public class OidcController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly ILogger<OidcController> _logger;
    private static readonly Dictionary<string, object> StateManager = new(); // Store AuthorizeState objects

    public OidcController(IUserManager userManager, ILogger<OidcController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("start")]
    public async Task<IActionResult> Start()
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return BadRequest("Plugin not initialized");
        }

        var options = new OidcClientOptions
        {
            Authority = config.OidEndpoint?.Trim(),
            ClientId = config.OidClientId?.Trim(),
            ClientSecret = config.OidSecret?.Trim(),
            RedirectUri = GetRedirectUri(),
            Scope = string.Join(" ", config.OidScopes)
        };

        try
        {
            var client = new OidcClient(options);
            var result = await client.PrepareLoginAsync().ConfigureAwait(false);

            // Store the authorize state for the callback
            var stateString = (string)result.GetType().GetProperty("State")?.GetValue(result);
            if (!string.IsNullOrEmpty(stateString))
            {
                StateManager[stateString] = result;
            }

            var startUrl = (string)result.GetType().GetProperty("StartUrl")?.GetValue(result);
            if (string.IsNullOrEmpty(startUrl))
            {
                _logger.LogError("Could not get StartUrl from OIDC result");
                return BadRequest("OIDC initialization failed");
            }

            return Redirect(startUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OIDC start error");
            return BadRequest("OIDC error: " + ex.Message);
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return BadRequest("Plugin not initialized");
        }

        try
        {
            var stateParam = Request.Query["state"].ToString();
            if (string.IsNullOrEmpty(stateParam) || !StateManager.TryGetValue(stateParam, out var storedState))
            {
                _logger.LogWarning("Invalid state: {State}", stateParam);
                return BadRequest("Invalid state");
            }

            var options = new OidcClientOptions
            {
                Authority = config.OidEndpoint?.Trim(),
                ClientId = config.OidClientId?.Trim(),
                ClientSecret = config.OidSecret?.Trim(),
                RedirectUri = GetRedirectUri(),
                Scope = string.Join(" ", config.OidScopes)
            };

            var client = new OidcClient(options);
            // Cast stored state to AuthorizeState - it's stored as object
            var authorizeState = (AuthorizeState)storedState;
            var result = await client.ProcessResponseAsync(Request.QueryString.Value, authorizeState).ConfigureAwait(false);

            if (result.IsError)
            {
                _logger.LogError("OIDC callback failed: {Error} - {ErrorDescription}", result.Error, result.ErrorDescription);
                return BadRequest("OIDC authentication failed");
            }

            // Get email from claims
            var email = result.User?.FindFirst("email")?.Value ?? 
                       result.User?.FindFirst("preferred_username")?.Value ??
                       result.User?.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email/username found in OIDC response");
                return BadRequest("No email/username found in OIDC response");
            }

            // Get or create user
            var user = _userManager.GetUserByName(email);
            if (user == null)
            {
                _logger.LogInformation("Creating new user: {Email}", email);
                user = await _userManager.CreateUserAsync(email).ConfigureAwait(false);
            }

            // Set authentication provider
            user.AuthenticationProviderId = "OIDC";

            // Get roles from claims
            var rolesClaimValue = result.User?.FindFirst(config.RoleClaim)?.Value;
            var roles = string.IsNullOrEmpty(rolesClaimValue)
                ? Array.Empty<string>()
                : rolesClaimValue.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Set permissions based on groups
            var isAdmin = roles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase));
            var isPowerUser = roles.Any(r => r.Equals("Power User", StringComparison.OrdinalIgnoreCase)) && !isAdmin;

            _logger.LogInformation("User {Email} authenticated. Admin: {IsAdmin}, PowerUser: {IsPowerUser}", email, isAdmin, isPowerUser);

            // Update user in database
            await _userManager.UpdateUserAsync(user).ConfigureAwait(false);

            StateManager.Remove(stateParam);

            // Redirect to Jellyfin main page
            return Redirect("/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OIDC callback error");
            return BadRequest("OIDC error: " + ex.Message);
        }
    }

    private string GetRedirectUri()
    {
        return $"{Request.Scheme}://{Request.Host}/api/oidc/callback";
    }
}
