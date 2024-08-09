﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Views;
using System;
using System.Threading.Tasks;

namespace SkEditor.API;

public class Windows : IWindows
{

    public MainWindow GetMainWindow()
    {
        return (MainWindow)(Application.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)
            .MainWindow;
    }

    public Window GetCurrentWindow()
    {
        var windows = (Application.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime).Windows;
        return windows.Count > 0 ? windows[^1] : GetMainWindow();
    }

    public async Task<ContentDialogResult> ShowDialog(string title,
        string message,
        object? icon = null,
        string? cancelButtonText = null,
        string primaryButtonText = "Okay")
    {
        static string? TryGetTranslation(string? input)
        {
            if (input == null)
                return null;

            var translation = Translation.Get(input);
            return translation == input ? input : translation;
        }

        Application.Current.TryGetResource("MessageBoxBackground", out var background);
        ContentDialog dialog = new()
        {
            Title = TryGetTranslation(title),
            Background = background as ImmutableSolidColorBrush,
            PrimaryButtonText = TryGetTranslation(primaryButtonText),
            CloseButtonText = TryGetTranslation(cancelButtonText),
        };

        icon = icon switch
        {
            IconSource iconSource => iconSource,
            Symbol symbol => new SymbolIconSource() { Symbol = symbol, FontSize = 40 },
            _ => icon
        };

        if (icon is not IconSource source)
        {
            if (icon is null)
            {
                source = null;
            }
            else
            {
                throw new ArgumentException("Icon must be of type IconSource, Symbol or SymbolIconSource.");
            }
        }
        if (source is FontIconSource fontIconSource)
            fontIconSource.FontSize = 40;
        else if (source is SymbolIconSource symbolIconSource)
            symbolIconSource.FontSize = 40;

        IconSourceElement iconElement = new()
        {
            IconSource = source,
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };

        double iconMargin = iconElement.IconSource is not null ? 24 : 0;

        var textBlock = new TextBlock()
        {
            Text = TryGetTranslation(message),
            FontSize = 16,
            Margin = new Thickness(Math.Max(10, iconMargin), 10, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400,
        };

        Grid.SetColumn(iconElement, 0);
        Grid.SetColumn(textBlock, 1);

        if (iconElement.IconSource is not null)
            grid.Children.Add(iconElement);
        grid.Children.Add(textBlock);

        dialog.Content = grid;

        return await dialog.ShowAsync(GetCurrentWindow());
    }

    public async Task ShowMessage(string title, string message)
    {
        await ShowDialog(title, message, Symbol.FlagFilled);
    }

    public async Task ShowError(string error)
    {
        await ShowDialog(Translation.Get("Error"), error, Symbol.AlertFilled);
    }

    public async Task<string?> AskForFile(FilePickerOpenOptions options)
    {
        var topLevel = GetCurrentWindow();
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

        return files[0]?.Path.AbsolutePath;
    }

    public void ShowWindow(Window window)
    {
        window.Show(GetCurrentWindow());
    }

    public Task ShowWindowAsDialog(Window window)
    {
        return window.ShowDialog(GetCurrentWindow());
    }
}