using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using UI.ViewModels;
using VirtualFileManagerDrive.Core;

namespace UI.Extensions;

public static class ObjectExtensions
{
    public static IEnumerable<DependencyObject> GetAllChildren(this DependencyObject parent, DependencyProperty? property = null, object? expectedValue = null)
    {
        if (property != null || expectedValue != null)
            if (property == null || expectedValue == null)
                throw new ArgumentNullException(property == null ? nameof(property) : nameof(expectedValue),
                    "Property and expectedValue must be not null, or null, but only both of them.");
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var directChild = (Visual)VisualTreeHelper.GetChild(parent, i);
            if (property == null || directChild.GetValue(property) == expectedValue)
                yield return directChild;
            foreach (var nestedChild in directChild.GetAllChildren())
                yield return nestedChild;
        }
    }

    public static void UpdateView(this ServerInstance server) => ((ServerInstanceViewModel?)server.View)?.Update();
    public static ServerInstanceViewModel GetView(this ServerInstance server) => (ServerInstanceViewModel?)server.View!;
}