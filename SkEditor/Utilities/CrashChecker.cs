﻿using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SkEditor.Utilities;
public class CrashChecker
{
    public async static Task<bool> CheckForCrash()
    {
        bool crash = Environment.GetCommandLineArgs().Any(arg => arg.Equals("--crash"));
        if (!crash) return false;

        await SkEditorAPI.Windows.ShowMessage("Oops!", "Sorry!\nIt looks that the app crashed, but don't worry, your files were saved.\nYou can check the logs for more details.\nIf you can, please report this on the Discord server.");

        string tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
        if (!Directory.Exists(tempPath)) return false;
        Directory.GetFiles(tempPath).ToList().ForEach(async file =>
        {
            TabViewItem tabItem = await FileBuilder.Build(Path.GetFileName(file), file);
            if (tabItem == null)
                return;
            tabItem.Tag = null;
            (SkEditorAPI.Files.GetTabView().TabItems as IList)?.Add(tabItem);
        });
        Directory.Delete(tempPath, true);
        return true;
    }
}
