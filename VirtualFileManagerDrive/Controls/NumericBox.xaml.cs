using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace VirtualFileManagerDrive.Controls;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public partial class NumericBox : INotifyPropertyChanged
{
    public double Number
    {
        get => (double)GetValue(NumberProperty);
        set
        {
            SetValue(NumberProperty, value = FixInRange(value));
            UpdateView();
            if (UseBoundedNumber)
                SetValue(BoundedNumberProperty, ConvertNumber2BoundedValue(value));
        }
    }
    
    public double? BoundedNumber
    {
        get => (double?)GetValue(BoundedNumberProperty);
        set
        {
            SetValue(BoundedNumberProperty, ConvertBound2NumberValue(value));
            SetValue(NumberProperty, value ?? InternallyMinNumber);
            UpdateView();
        }
    }

    public bool UseBoundedNumber
    {
        get => (bool)GetValue(UseBoundedNumberProperty);
        set => SetValue(UseBoundedNumberProperty, value);
    }

    private bool _renderNumberAsNormal;
    
    public string RenderedNumber
    {
        get => _renderNumberAsNormal ? $"{Number}" : GetDisplay(out var display)
            ? string.Format(EscapeFormatting(NumberFormatForDisplay), display, Number)
            : string.Format(EscapeFormatting(NumberFormat), Number, display);
        set => Number = GetNumberFromDisplay(value);
    }

    public string NumberFormat
    {
        get => (string)GetValue(NumberFormatProperty);
        set => SetValue(NumberFormatProperty, value);
    }
    
    public string NumberFormatForDisplay
    {
        get => (string)GetValue(NumberFormatForDisplayProperty);
        set => SetValue(NumberFormatForDisplayProperty, value);
    }
    
    public ListDictionary SpecialRenderedNumbersList
    {
        get => (ListDictionary)GetValue(SpecialRenderedNumbersListProperty);
        set => SetValue(SpecialRenderedNumbersListProperty, value);
    }
    
    public double? DefaultValueWhenBelowInternallyMinNumber
    {
        get => (double?)GetValue(DefaultValueWhenBelowInternallyMinNumberProperty);
        set => SetValue(DefaultValueWhenBelowInternallyMinNumberProperty, value);
    }
    
    public double Steps
    {
        get => (double)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }
    
    public double MinNumber
    {
        get => (double)GetValue(MinNumberProperty);
        set => SetValue(MinNumberProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => InputBox.TextAlignment;
        set => InputBox.TextAlignment = value;
    }
    
    public double InternallyMinNumber
    {
        get => (double)(GetValue(InternallyMinNumberProperty) ?? MinNumber);
        set => SetValue(InternallyMinNumberProperty, value);
    }
    
    public double MaxNumber
    {
        get => (double)GetValue(MaxNumberProperty);
        set => SetValue(MaxNumberProperty, value);
    }
    
    public double? ExcludedFrom
    {
        get => (double?)GetValue(ExcludeFromProperty);
        set => SetValue(ExcludeFromProperty, value);
    }
    
    public double? ExcludedTo
    {
        get => (double?)GetValue(ExcludeToProperty);
        set => SetValue(ExcludeToProperty, value);
    }

    public NumericBox()
    {
        InitializeComponent();
        // Default
        InputBox.TextAlignment = TextAlignment.Center;
    }

    private static string EscapeFormatting(string str)
    {
        return str.Replace("\\[", "[")
            .Replace("\\[", "[")
            .Replace('[', '{')
            .Replace(']', '}');
    }

    private List<double> GetExcludedNumbersRange()
    {
        if (ExcludedFrom == null && ExcludedTo == null)
            return [];
        var min = Math.Min((double)ExcludedFrom!, (double)ExcludedTo!);
        var max = Math.Max((double)ExcludedFrom!, (double)ExcludedTo!);
        if (min == max)
            return [min];
        var steps = Steps;
        List<double> array = [];
        var next = min;
        while (next <= max)
        {
            array.Add(next);
            next += steps;
        }
        return array;
    }

    private void UpdateView() => RaisePropertyChanged(nameof(RenderedNumber));

    private T CheckUseBoundedNumber<T>(T obj)
    {
        if (UseBoundedNumber)
            return obj;
        throw new ArgumentException("Can't use BoundedNumber because UseBoundedNumber property is set to false!");
    }

    private double? ConvertNumber2BoundedValue(double value) =>
        CheckUseBoundedNumber(value < InternallyMinNumber ? DefaultValueWhenBelowInternallyMinNumber : value);
    
    private double ConvertBound2NumberValue(double? value) =>
        CheckUseBoundedNumber(value != null ? FixInRange((double)value, true) :
        value == DefaultValueWhenBelowInternallyMinNumber ? MinNumber : InternallyMinNumber);
    
    private void CheckButtons(double newValue)
    {
        IncreaseButton.IsEnabled = !(newValue == Math.Min(newValue + Steps, MaxNumber) && newValue == MaxNumber);
        DecreaseButton.IsEnabled = !(newValue == Math.Max(newValue - Steps, MinNumber) && newValue == MinNumber);
    }

    private bool GetDisplay(out string display)
    {
        foreach (var entry in SpecialRenderedNumbersList.Cast<DictionaryEntry>().Where(entry =>
                     entry.Key.ToString() == Number.ToString(CultureInfo.InvariantCulture)))
        {
            display = entry.Value?.ToString() ?? "Err";
            return true;
        }
        display = $"{Number}";
        return false;
    }

    private double FixInRange(double d, bool useInternalMinNumber = false) =>
        Math.Min(Math.Max(d, useInternalMinNumber ? CheckUseBoundedNumber(InternallyMinNumber) : MinNumber), MaxNumber);

    private double GetNumberFromDisplay(string displayOrNumber)
    {
        if (displayOrNumber.Equals("min", StringComparison.CurrentCultureIgnoreCase))
            return MinNumber;
        if (displayOrNumber.Equals("max", StringComparison.CurrentCultureIgnoreCase))
            return MaxNumber;
        foreach (var key in SpecialRenderedNumbersList.Keys)
        {
            var value = SpecialRenderedNumbersList[key];
            if ((string?)value != displayOrNumber) continue;
            return double.Parse(key.ToString() ?? string.Empty);
        }
        return double.TryParse(displayOrNumber, out var d) ? FixInRange(d) : MinNumber;
    }

    private double NextValue(Func<double> func)
    {
        var excluded = GetExcludedNumbersRange();
        if (excluded.Contains(MinNumber) || excluded.Contains(MaxNumber))
            throw new ArgumentException("Excluded numbers list cannot contain MinNumber and/or MaxNumber");
        Number = func.Invoke();
        while (excluded.Contains(Number))
            Number = func.Invoke();

        return Number;
    }

    private void DecreaseButton_OnClick(object sender, RoutedEventArgs? e)
    {
        if (sender is RepeatButton { IsEnabled: true })
            CheckButtons(NextValue(() => Math.Max(Number - Steps, MinNumber)));
    }

    private void IncreaseButton_OnClick(object sender, RoutedEventArgs? e)
    {
        if (sender is RepeatButton { IsEnabled: true })
            CheckButtons(NextValue(() => Math.Min(Number + Steps, MaxNumber)));
    }

    private void NumericBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        // A workaround setting Number to BoundedNumber, it won't work while initializing,
        // because it gives out default value that is set inside PropertyMetadata.
        var presentationSource = PresentationSource.FromVisual((Visual)sender);
        if (presentationSource != null)
            presentationSource.ContentRendered += (_, _) =>
            {
                if (UseBoundedNumber)
                    Number = BoundedNumber ?? MinNumber;
            };
    }

    private void Grid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        switch (e.Delta)
        {
            case > 0:
                IncreaseButton_OnClick(IncreaseButton, null);
                break;
            case < 0:
                DecreaseButton_OnClick(DecreaseButton, null);
                break;
        }
    }
    
    public static readonly DependencyProperty NumberProperty =
        DependencyProperty.Register(nameof(Number), typeof(double), 
            typeof(NumericBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    
    public static readonly DependencyProperty BoundedNumberProperty =
        DependencyProperty.Register(nameof(BoundedNumber), typeof(double?), 
            typeof(NumericBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    
    public static readonly DependencyProperty NumberFormatForDisplayProperty =
        DependencyProperty.Register(nameof(NumberFormatForDisplay), typeof(string), 
            typeof(NumericBox), new PropertyMetadata("{0}")
        );
    
    public static readonly DependencyProperty NumberFormatProperty =
        DependencyProperty.Register(nameof(NumberFormat), typeof(string), 
            typeof(NumericBox), new PropertyMetadata("{0}")
        );
    
    public static readonly DependencyProperty SpecialRenderedNumbersListProperty =
        DependencyProperty.Register(nameof(SpecialRenderedNumbersList), typeof(ListDictionary), 
            typeof(NumericBox), new PropertyMetadata(new ListDictionary())
        );
    
    public static readonly DependencyProperty DefaultValueWhenBelowInternallyMinNumberProperty =
        DependencyProperty.Register(nameof(DefaultValueWhenBelowInternallyMinNumber), typeof(double?), 
            typeof(NumericBox), new PropertyMetadata(defaultValue:null)
        );
    
    public static readonly DependencyProperty StepsProperty =
        DependencyProperty.Register(nameof(Steps), typeof(double), 
            typeof(NumericBox), new PropertyMetadata(1d)
        );
    
    public static readonly DependencyProperty MinNumberProperty =
        DependencyProperty.Register(nameof(MinNumber), typeof(double), 
            typeof(NumericBox), new PropertyMetadata(-1000d)
        );
    
    public static readonly DependencyProperty InternallyMinNumberProperty =
        DependencyProperty.Register(nameof(InternallyMinNumber), typeof(double?), 
            typeof(NumericBox), new PropertyMetadata(defaultValue:null)
        );
    
    public static readonly DependencyProperty MaxNumberProperty =
        DependencyProperty.Register(nameof(MaxNumber), typeof(double), 
            typeof(NumericBox), new PropertyMetadata(1000d)
        );
    
    public static readonly DependencyProperty ExcludeFromProperty =
        DependencyProperty.Register(nameof(ExcludeFromProperty), typeof(double?), 
            typeof(NumericBox), new PropertyMetadata(defaultValue:null)
        );
    
    public static readonly DependencyProperty ExcludeToProperty =
        DependencyProperty.Register(nameof(ExcludeToProperty), typeof(double?), 
            typeof(NumericBox), new PropertyMetadata(defaultValue:null)
        );
    
    public static readonly DependencyProperty UseBoundedNumberProperty =
        DependencyProperty.Register(nameof(UseBoundedNumberProperty), typeof(bool), 
            typeof(NumericBox), new PropertyMetadata(false)
        );
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Input_OnGotFocus(object sender, RoutedEventArgs e)
    {
        _renderNumberAsNormal = true;
        UpdateView();
    }

    private void Input_OnLostFocus(object sender, RoutedEventArgs e)
    {
        _renderNumberAsNormal = false;
        Number = GetNumberFromDisplay(((TextBox)sender).Text);
        UpdateView();
    }
}