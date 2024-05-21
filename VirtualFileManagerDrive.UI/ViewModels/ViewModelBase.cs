using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UI.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        VerifyPropertyName(propertyName);
        var handler = PropertyChanged;
        if (handler == null) return;
        var e = new PropertyChangedEventArgs(propertyName); 
        handler(this, e);
    }
    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        // Verify that the property name matches a real, 
        // public, instance property on this object. 
        if (TypeDescriptor.GetProperties(this)[propertyName] != null) return;
        var msg = "Invalid property name: " + propertyName;
        if (ThrowOnInvalidPropertyName) 
            throw new Exception(msg);
        Debug.Fail(msg);
    }
    protected bool ThrowOnInvalidPropertyName { get; private set; }
}