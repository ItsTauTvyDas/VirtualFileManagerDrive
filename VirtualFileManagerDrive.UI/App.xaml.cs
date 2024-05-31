using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Windows;
using VirtualFileManagerDrive.Core;

namespace UI;

public partial class App
{
    private static void ExceptionHandler(object? sender, UnhandledExceptionEventArgs args)
    {
        var exception = (Exception) args.ExceptionObject;
        if (MessageBox.Show(
                (Debugger.IsAttached ? exception.ToString() : exception.Message) +
                $"\n\nDo you want to copy the {(Debugger.IsAttached ? "exception" : "error")}?",
                "An error occurred!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
            ) == MessageBoxResult.Yes)
            Clipboard.SetText(Debugger.IsAttached ? exception.ToString() : exception.Message);
    }
    
    public App()
    {
        ServerInstance.ExceptionHandler += ExceptionHandler;

        if (!Debugger.IsAttached)
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
        ApplicationSettings.Load();
    }
}