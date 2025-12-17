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

    public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6");

    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = "JellyfinOIDCPlugin.web.configurationpage.html"
            }
        };
    }
}
