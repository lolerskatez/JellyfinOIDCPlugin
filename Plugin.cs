#nullable enable

using System;
using System.Collections.Generic;
using JellyfinOIDCPlugin.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace JellyfinOIDCPlugin;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "OIDC Authentication";

    public override Guid Id => Guid.Parse("7f8b8d9e-1234-5678-90ab-cdef12345678");

    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "OIDC Configuration",
                EmbeddedResourcePath = "configurationpage.html",
                DisplayName = "OIDC Authentication"
            }
        };
    }
}
