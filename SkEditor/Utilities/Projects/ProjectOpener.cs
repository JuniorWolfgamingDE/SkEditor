﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Projects.Elements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities.Projects;
public static class ProjectOpener
{
    public static Folder? ProjectRootFolder = null;
    private static ExplorerSidebarPanel Panel => AddonLoader.GetCoreAddon().ProjectPanel.Panel;
    public static TreeView FileTreeView => Panel.FileTreeView;
    private static StackPanel NoFolderMessage => Panel.NoFolderMessage;

    public static async void OpenProject(string? path = null)
    {
        string folder = string.Empty;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            TopLevel topLevel = TopLevel.GetTopLevel(SkEditorAPI.Windows.GetMainWindow());

            IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            });

            if (folders.Count == 0)
            {
                NoFolderMessage.IsVisible = ProjectRootFolder == null;
                return;
            }
            folder = folders[0].Path.AbsolutePath;
        }
        else
        {
            folder = path;
        }

        folder.FixLinuxPath();

        NoFolderMessage.IsVisible = false;

        ProjectRootFolder = new Folder(folder) { IsExpanded = true };
        FileTreeView.ItemsSource = new ObservableCollection<StorageElement> { ProjectRootFolder };

        //FileSystemWatcher watcher = new(folder)
        //{
        //    EnableRaisingEvents = true
        //};

        //watcher.Renamed += (sender, e) =>
        //{
        //    string path = Uri.UnescapeDataString(e.OldFullPath).Replace("/", "\\");
        //    ProjectRootFolder.GetItemByPath(path)?.RenameElement(e.Name, false);
        //};

        static void HandleTapped(TappedEventArgs e)
        {
            if (e.Source is not Border border) return;
            var treeViewItem = border.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            if (treeViewItem is null) return;
            var storageElement = treeViewItem.DataContext as StorageElement;
            storageElement?.HandleClick();
        }

        FileTreeView.DoubleTapped += (sender, e) =>
        {
            if (SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
                return;
            HandleTapped(e);
        };

        FileTreeView.Tapped += (sender, e) =>
        {
            if (!SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
                return;
            HandleTapped(e);
        };
    }

    #region Sorting

    private static void SortTabItem(TreeViewItem parent)
    {
        var folders = parent.Items
            .OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFolder)
            .OrderBy(item => item.Header);

        var files = parent.Items
            .OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFile)
            .OrderBy(item => item.Header);

        parent.Items.Clear();
        foreach (var folder in folders) parent.Items.Add(folder);
        foreach (var file in files) parent.Items.Add(file);
    }

    #endregion
}
