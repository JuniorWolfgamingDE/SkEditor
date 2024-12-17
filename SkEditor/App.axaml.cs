﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SkEditor/Logs/log-.txt"), rollingInterval: RollingInterval.Minute)
            .WriteTo.Sink(new LogsHandler())
            .CreateLogger();

        Dispatcher.UIThread.UnhandledException += async (sender, e) =>
        {
            e.Handled = true;
            string? source = e.Exception.Source;
            if (AddonLoader.DllNames.Contains(source + ".dll"))
            {
                Log.Error(e.Exception, $"An error occured in an addon: {source}");
                await SkEditorAPI.Windows.ShowMessage("Error", $"An error occured in an addon: {source}\n\n{e.Exception.Message}");
                return;
            }

            string message = "Application crashed!";
            Log.Fatal(e.Exception, message);
            Console.Error.WriteLine(e);
            Console.Error.WriteLine(message);

            await SkEditorAPI.Core.SaveData();
            AddonLoader.SaveMeta();

            var fullException = e.Exception.ToString();
            var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(fullException));
            await Task.Delay(500);
            Process.Start(Environment.ProcessPath, "--crash " + encodedMessage);
            Environment.Exit(1);
        };

        Mutex mutex = new(true, "{217619cc-ff9d-438b-8a0a-348df94de61b}");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            bool isFirstInstance;
            try
            {
                isFirstInstance = mutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Debug(ex, "Abandoned mutex");
                isFirstInstance = true;
            }

            if (isFirstInstance)
            {
                try
                {
                    SkEditorAPI.Core.SetStartupArguments(desktop.Args ?? []);

                    MainWindow mainWindow = new();
                    desktop.MainWindow = mainWindow;

                    NamedPipeServer.Start();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error creating SkEditor");
                    desktop.Shutdown();
                }

                desktop.Exit += (sender, e) =>
                {
                    mutex.ReleaseMutex();
                    mutex.Dispose();
                };
            }
            else
            {
                var args = Environment.GetCommandLineArgs();
                try
                {
                    using NamedPipeClientStream namedPipeClientStream = new("SkEditor");
                    await namedPipeClientStream.ConnectAsync();
                    if (args != null && args.Length > 1)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, args.Skip(1)));
                        namedPipeClientStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        namedPipeClientStream.WriteByte(0);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Error connecting to named pipe");
                }

                desktop.Shutdown();
            }
        }
    }
}