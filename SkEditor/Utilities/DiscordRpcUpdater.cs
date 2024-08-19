﻿using DiscordRPC;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;
public static class DiscordRpcUpdater
{
    private const string ApplicationId = "1152625623777431662";
    private static DiscordRpcClient? _client;

    public static void Initialize()
    {
        if (!SkEditorAPI.Core.GetAppConfig().IsDiscordRpcEnabled) return;

        _client = new DiscordRpcClient(ApplicationId);
        _client.Initialize();

        _client.SetPresence(new RichPresence()
        {
            Timestamps = Timestamps.Now,
            Assets = new Assets()
            {
                LargeImageKey = "image_large",
                LargeImageText = "SkEditor",
            },
        });

        SkEditorAPI.Files.GetTabView().SelectionChanged += (sender, args) =>
        {
            if (!_client.IsInitialized) return;
            if (SkEditorAPI.Files.IsFileOpen())
            {
                TabViewItem tab = SkEditorAPI.Files.GetTabView().SelectedItem as TabViewItem;
                _client.UpdateDetails(Translation.Get("DiscordRpcEditing").Replace("{0}", tab.Header.ToString().TrimEnd('*')));
            }
            else
            {
                _client.UpdateDetails("");
            }
        };
    }

    public static void Uninitialize()
    {
        if (_client == null)
            return;

        _client.ClearPresence();
        _client.Dispose();
    }
}
