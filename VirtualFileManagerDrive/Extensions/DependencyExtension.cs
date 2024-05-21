using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace VirtualFileManagerDrive.Extensions;

public static class DependencyExtension
{
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.RegisterAttached(
            "IsRequired",
            typeof(bool),
            typeof(DependencyExtension),
            new PropertyMetadata(default(bool)));
    public static void SetIsRequired(UIElement element, bool value) => element.SetValue(IsRequiredProperty, value);
    public static bool GetIsRequired(UIElement element) => (bool)element.GetValue(IsRequiredProperty);
    
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.RegisterAttached(
            "HasError",
            typeof(bool),
            typeof(DependencyExtension),
            new PropertyMetadata(default(bool)));
    public static void HasErrorRequired(UIElement element, bool value) => element.SetValue(HasErrorProperty, value);
    public static bool HasErrorRequired(UIElement element) => (bool)element.GetValue(HasErrorProperty);
    
    public static readonly DependencyProperty MinTextLengthProperty =
        DependencyProperty.RegisterAttached(
            "MinTextLength",
            typeof(int),
            typeof(DependencyExtension),
            new PropertyMetadata(-1));
    public static void SetMinTextLength(UIElement element, int value) => element.SetValue(MinTextLengthProperty, value);
    public static int GetMinTextLength(UIElement element) => (int)element.GetValue(MinTextLengthProperty);

    static DependencyExtension()
    {
        RedBorderTrigger = new DataTrigger()
        {
            Binding = new Binding("IsKeyboardFocused"),
            Value = true
        };
        RedBorderTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Red));
    }
    
    private static readonly DataTrigger RedBorderTrigger;
    public static bool CheckRequiredElements(Window window, DependencyObject obj, out DependencyObject? faultyObj,
        DependencyObject? oldFaultyObj, string messageBoxCaption = "")
    {
        var elements = obj.GetAllChildren(IsRequiredProperty, true);
        var objects = elements as DependencyObject[] ?? elements.ToArray();
        foreach (var element in objects)
        {
            try
            {
                if (!(bool)element.GetValue(IsRequiredProperty)) continue;
                var min = (int)element.GetValue(MinTextLengthProperty);
                var text = (string)element.GetValue(TextBox.TextProperty);
                if ((min == -1 && text.Length > 0) || (min > 0 && text.Length >= min)) continue;
                var message = $"'{element.GetValue(FrameworkElement.TagProperty)}' is required!";
                if (min != -1)
                    message += $" Minimum input length is {min}.";
                MessageBox.Show(
                    window,
                    message,
                    messageBoxCaption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                faultyObj = element;
                oldFaultyObj?.ClearValue(Control.BorderBrushProperty);
                oldFaultyObj?.ClearValue(HasErrorProperty);
                ((ControlTemplate?)oldFaultyObj?.GetValue(Control.TemplateProperty))?.Triggers
                    .Remove(RedBorderTrigger);
                //oldFaultyObj?.ClearValue(Control.TemplateProperty);
                faultyObj.SetValue(Control.BorderBrushProperty, Brushes.Red);
                faultyObj.SetValue(HasErrorProperty, true);
                // //faultyObj.SetValue(Control.TemplateProperty, Control.TemplateProperty.DefaultMetadata.DefaultValue);
                // var prop = faultyTriggers.GetType().GetField("_sealed",
                //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                // prop?.SetValue(faultyTriggers, false);
                // // faultyTriggers.Add(RedBorderTrigger);
                // prop?.SetValue(faultyTriggers, true);
                return false;
            }
            catch (InvalidOperationException) {}
        }

        faultyObj = null;
        return true;
    }
}