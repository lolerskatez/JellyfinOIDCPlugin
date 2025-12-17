using System;
using System.IO;
using System.Reflection;
using MediaBrowser.Common.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JellyfinOIDCPlugin.Controllers;

[ApiController]
[Route("api/oidc")]
public class OidcStaticController : ControllerBase
{
    private readonly ILogger<OidcStaticController> _logger;

    public OidcStaticController(ILogger<OidcStaticController> logger)
    {
        _logger = logger;
    }

    [HttpGet("login.js")]
    public IActionResult GetLoginScript()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("JellyfinOIDCPlugin.web.oidc-login.js");
            if (stream == null)
            {
                _logger.LogWarning("Login script resource not found");
                return NotFound();
            }

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            
            return Content(content, "application/javascript");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving login script");
            return StatusCode(500, "Error loading login script");
        }
    }
}
