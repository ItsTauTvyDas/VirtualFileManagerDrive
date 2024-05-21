using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using VirtualDrive;

namespace VirtualFileManagerDrive;

public partial class App
{
    public App()
    {
        if (!Debugger.IsAttached)
            AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
            {
                if (MessageBox.Show(
                        args.Exception+"\n\nDo you want to copy the exception?",
                    "An exception occurred!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error
                    ) == MessageBoxResult.Yes)
                    Clipboard.SetText(args.Exception.ToString());
            };
        ApplicationSettings.Load();
    }
}