namespace VirtualFileManagerDrive.Common;

public class AdditionalData(
    string groupBoxHeader,
    string title,
    object dataValue,
    Action<object>? onChanged = null,
    bool isNumber = false,
    bool inline = false,
    int? fixedWidth = null)
{
    public readonly string Header = groupBoxHeader;
    public readonly string Title = title;
    public readonly bool Inline = inline;
    public readonly bool IsNumber = isNumber;
    public readonly int? FixedWidth = fixedWidth;
    public object Value
    {
        get => dataValue;
        set
        {
            dataValue = value;
            onChanged?.Invoke(dataValue);
        }
    }
}