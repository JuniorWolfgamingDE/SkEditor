﻿using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Views;

public partial class MainWindow : AppWindow
{
    public static MainWindow Instance { get; private set; }
    
    public BottomBarControl GetBottomBar() => BottomBar;

    public MainWindow()
    {
        InitializeComponent();

        WindowStyler.Style(this);
        ThemeEditor.LoadThemes();
        AddEvents();

        Translation.LoadDefaultLanguage();
        Translation.ChangeLanguage(SkEditorAPI.Core.GetAppConfig().Language);

        Instance = this;
    }

    private void AddEvents()
    {
        TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
        TabControl.TabCloseRequested += (sender, e) => FileCloser.CloseFile(e);
        TemplateApplied += OnWindowLoaded;
        Closing += OnClosing;

        Activated += (sender, e) => ChangeChecker.Check();

        KeyDown += (sender, e) =>
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                FileHandler.SwitchTab((int)e.Key - 35);
            }
        };

        DragDrop.SetAllowDrop(this, true);
        DragDrop.DropEvent.AddClassHandler(FileHandler.FileDropAction);
    }

    public void ReloadUiOfAddons()
    {
        MainMenu.ReloadAddonsMenus();
        BottomBar.ReloadBottomIcons();
        SideBar.ReloadPanels();
    }

    public bool AlreadyClosed { get; set; } = false;
    private async void OnClosing(object sender, WindowClosingEventArgs e)
    {
        if (AlreadyClosed) return;

        ThemeEditor.SaveAllThemes();
        SkEditorAPI.Core.GetAppConfig().Save();

        e.Cancel = true;
        if (!SkEditorAPI.Core.GetAppConfig().EnableSessionRestoring)
        {
            List<TabViewItem> unsavedFiles = ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>().Where(item => item.Header.ToString().EndsWith('*')).ToList();
            if (unsavedFiles.Count == 0)
            {
                e.Cancel = false;
                return;
            }

            ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Attention"), Translation.Get("ClosingProgramWithUnsavedFiles"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled });
            if (result == ContentDialogResult.Primary)
            {
                AlreadyClosed = true;
                Close();
            }
        }
        else
        {
            await SessionRestorer.SaveSession();
            SkEditorAPI.Logs.Debug("Session saved.");
            AlreadyClosed = true;
            Close();
        }
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        AddonLoader.Load();
        Utilities.Files.FileTypes.RegisterDefaultAssociations();
        SideBar.ReloadPanels();

        await ThemeEditor.SetTheme(ThemeEditor.CurrentTheme);

        bool sessionFilesAdded = false;
        if (SkEditorAPI.Core.GetAppConfig().EnableSessionRestoring) sessionFilesAdded = await SessionRestorer.RestoreSession();

        string[] startupFiles = SkEditorAPI.Core.GetStartupArguments();
        if (startupFiles.Length == 0 && !await CrashChecker.CheckForCrash() && !sessionFilesAdded) 
            (SkEditorAPI.Files as Files).AddWelcomeTab();
        startupFiles.ToList().ForEach(FileHandler.OpenFile);

        Dispatcher.UIThread.Post(() =>
        {
            SyntaxLoader.LoadAdvancedSyntaxes();
            DiscordRpcUpdater.Initialize();

            if (SkEditorAPI.Core.GetAppConfig().CheckForUpdates) UpdateChecker.Check();

            Tutorial.ShowTutorial();
            BottomBar.UpdatePosition();
            ChangelogChecker.Check();
        });
    }
}