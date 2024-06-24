﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Styling;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

public class Core : ICore
{
    private AppConfig? _appConfig;
    private string[] _startupArguments = null!;
    
    public AppConfig GetAppConfig()
    {
        return _appConfig ??= AppConfig.Load();
    }

    public Version GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return new Version(version.Major, version.Minor, version.Build);
    }

    public string[] GetStartupArguments()
    {
        return _startupArguments ?? [];
    }
    
    public void SetStartupArguments(string[]? args)
    {
        _startupArguments = args ?? [];
    }

    public object? GetApplicationResource(string key)
    {
        Application.Current.TryGetResource(key, ThemeVariant.Dark, out var resource);
        return resource;
    }

    public void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public bool IsDeveloperMode()
    {
#pragma warning disable 162
#if DEBUG
        return true;
#endif

        return GetAppConfig().IsDevModeEnabled;
        #pragma warning restore 162
    }

    public void SaveData()
    {
        List<OpenedFile> files = SkEditorAPI.Files.GetOpenedEditors();

        files.ForEach(file =>
        {
            string path = file.Path;
            if (string.IsNullOrEmpty(path))
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
                Directory.CreateDirectory(tempPath);
                string header = file.Header;
                path = Path.Combine(tempPath, header);
            }
            string textToWrite = file.Editor.Text;
            using StreamWriter writer = new(path, false);
            writer.Write(textToWrite);
        });

        GetAppConfig().Save();
    }
}