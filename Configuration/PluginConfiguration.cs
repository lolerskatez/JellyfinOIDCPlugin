using MediaBrowser.Model.Plugins;

namespace JellyfinOIDCPlugin.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        OidEndpoint = string.Empty;
        OidClientId = string.Empty;
        OidSecret = string.Empty;
        OidScopes = new[] { "openid", "profile", "email", "groups" };
        RoleClaim = "groups";
    }

    public string OidEndpoint { get; set; }

    public string OidClientId { get; set; }

    public string OidSecret { get; set; }

    public string[] OidScopes { get; set; }

    public string RoleClaim { get; set; }
}
