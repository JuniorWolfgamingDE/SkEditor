﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

/// <summary>
/// Interface for managing SkEditor's files & editors.
/// </summary>
public interface IFiles
{

    /// <summary>
    /// Check if any file  is currently opened and shown
    /// as the active tab view item.
    /// </summary>
    public bool IsFileOpen();

    /// <summary>
    /// Check if any text editor is currently opened
    /// and shown as the active tab view item.
    /// </summary>
    public bool IsEditorOpen();

    /// <summary>
    /// Get all current opened files
    /// </summary>
    public List<OpenedFile> GetOpenedFiles();

    /// <summary>
    /// Get all current opened tab view items
    /// </summary>
    public List<TabViewItem> GetOpenedTabs();

    /// <summary>
    /// Get all current opened files with text editors
    /// </summary>
    public List<OpenedFile> GetOpenedEditors();

    /// <summary>
    /// Get the current opened file.
    /// </summary>
    public OpenedFile GetCurrentOpenedFile();

    /// <summary>
    /// Get the current opened file, as the currently selected tab view item
    /// </summary>
    public TabViewItem GetCurrentTabViewItem();

    /// <summary>
    /// Get the main tab view, used by SkEditor to displays opened files.
    /// </summary>
    public TabView GetTabView();

    /// <summary>
    /// Manually add a new tab/file into the opened files.
    /// SkEditor will manually handle all the needs, e.g. adding
    /// a new <see cref="OpenedFile"/> to the registry.
    /// </summary>
    /// <param name="content">The control that represent the tab view item's content</param>
    /// <param name="header">The (mainly text) header of the created tab view item</param>
    /// <param name="select">Should the new created tab be selected right after its creation or not.</param>
    public void AddCustomTab(object header, Control content, bool select = true);

    /// <summary>
    /// Select the desired opened file's tab item.
    /// </summary>
    /// <param name="entity">The opened file or tab view item to select</param>
    public void Select(object entity);

    /// <summary>
    /// Close a <see cref="OpenedFile"/> or <see cref="TabViewItem"/> from the list of tabs.
    /// </summary>
    /// <param name="entity">The <see cref="OpenedFile"/> or <see cref="TabViewItem"/> to be removed</param>
    public Task Close(object entity);

    /// <summary>
    /// Add a "New File" tab with the desired content.
    /// </summary>
    /// <param name="content">The desired content of the new file</param>
    public void NewFile(string content = "");
    
    /// <summary>
    /// Open the provided file path into SkEditor.
    /// </summary>
    /// <param name="path">The path of the target file.</param>
    public void OpenFile(string path);

    /// <summary>
    /// Get a (possibly-null) opened file with the given path.
    /// </summary>
    /// <param name="path">The path you're looking for</param>
    public OpenedFile? GetOpenedFileByPath(string path);

    /// <summary>
    /// Save the desired tab view item or opening file, asking
    /// the path if it was a new/unsaved file.
    /// </summary>
    /// <param name="entity">The tab view item or opened file to save.</param>
    public Task Save(object entity, bool saveAs = false);

    /// <summary>
    /// Close a lot of tab view items according to the given
    /// <see cref="FileCloseAction"/>.
    /// </summary>
    public void BatchClose(FileCloseAction closeAction);

    public enum FileCloseAction
    {
        Unsaved,
        AllExceptCurrent,
        AllRight,
        AllLeft,
        All
    }
    
}