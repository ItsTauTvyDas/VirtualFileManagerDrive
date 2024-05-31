using System.Windows;
using System.Windows.Controls;

namespace UI.Helper;

public static class AutoScrollBehavior
{
    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior),
            new PropertyMetadata(false, AutoScrollPropertyChanged));


    private static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not ScrollViewer scrollViewer) return;
        if((bool)args.NewValue)
        {
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.ScrollToEnd();
            return;
        }
        scrollViewer.ScrollChanged-= ScrollViewer_ScrollChanged;
    }

    private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentHeightChange == 0) return;
        var scrollViewer = sender as ScrollViewer;
        scrollViewer?.ScrollToBottom();
    }

    public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
}