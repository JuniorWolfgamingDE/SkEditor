﻿using Avalonia.Controls;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkEditor.API;

public interface ISkEditorAPI
{
    public MainWindow GetMainWindow();

    public string[] GetStartupFiles();

    public Menu GetMenu();

    public bool IsFileOpen();

    public bool IsFile(TabViewItem tabItem);

    public TextEditor? GetTextEditor();

    public OpenedFile? GetOpenedFile();

    public TabView GetTabView();

    public void OpenUrl(string url);
    public void OpenFolder(string path);

    public void ShowMessage(string title, string message, Window window);
    public void ShowMessage(string title, string message);

    public void ShowError(string message);

    public Task<ContentDialogResult> ShowMessageWithIcon(string title, string message, IconSource icon, string iconColor = "#ffffff", string primaryButtonContent = "ConfirmButton", string closeButtonContent = "CancelButton", bool primaryButton = true);
    public Task<ContentDialogResult> ShowAdvancedMessage(string title, string message, string primaryButtonContent = "ConfirmButton", string closeButtonContent = "CancelButton", bool primaryButton = true);

    public AppConfig GetAppConfig();

    public void Debug(string message);

    public void Log(string message, bool bottomBarInfo = false);
    public void SendToBottomBar(object message);

    public void SaveData();

    public List<TextEditor> GetOpenedEditors();

    #region Events

    public event EventHandler Closed;
    public void OnClosed();

    public event EventHandler<TextEditorEventArgs> FileCreated;
    public void OnFileCreated(TextEditor textEditor);

    public event EventHandler<TextEditorCancelEventArgs> FileClosing;
    public bool OnFileClosing(TextEditor textEditor);

    public event EventHandler SettingsOpened;
    public void OnSettingsOpened();

    #endregion
}