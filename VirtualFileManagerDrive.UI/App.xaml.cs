using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Windows;
using VirtualFileManagerDrive.Core;

namespace UI;

public partial class App
{
    private static void ExceptionHandler(object? sender, FirstChanceExceptionEventArgs args)
    {
        if (MessageBox.Show(
                args.Exception+"\n\nDo you want to copy the exception?",
                "An exception occurred!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
            ) == MessageBoxResult.Yes)
            Clipboard.SetText(args.Exception.ToString());
    }
    
    public App()
    {
        ServerInstance.ExceptionHandler += ExceptionHandler;

        if (!Debugger.IsAttached)
            AppDomain.CurrentDomain.FirstChanceException += ExceptionHandler;
        ApplicationSettings.Load();
    }
}