namespace WinForms.Controls;

public enum FieldKind { Text, Int, Decimal, Date }

// Describes one editable field on an entity form: which model property it maps
// to, its label, the kind of input control, and whether it is read-only.
// EntityView<T> uses these to build the form and to read/write values.
public class FormField
{
    public string Property { get; }
    public string Label { get; }
    public FieldKind Kind { get; }
    public bool ReadOnly { get; }
    public Control Input { get; private set; } = null!;

    public FormField(string property, string label, FieldKind kind, bool readOnly = false)
    {
        Property = property;
        Label = label;
        Kind = kind;
        ReadOnly = readOnly;
    }

    public Control Build()
    {
        Input = Kind switch
        {
            FieldKind.Int => new NumericUpDown { Minimum = 0, Maximum = 1_000_000, Width = 200 },
            FieldKind.Decimal => new NumericUpDown { Minimum = 0, Maximum = 100_000_000, DecimalPlaces = 2, Increment = 10, Width = 200 },
            FieldKind.Date => new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 200 },
            _ => new TextBox { Width = 200 }
        };
        Input.Font = UiTheme.Base;
        if (ReadOnly)
        {
            Input.Enabled = false;
            if (Input is TextBox tb) tb.BackColor = Color.FromArgb(238, 240, 243);
        }
        return Input;
    }

    public object? GetValue() => Kind switch
    {
        FieldKind.Int => (int)((NumericUpDown)Input).Value,
        FieldKind.Decimal => ((NumericUpDown)Input).Value,
        FieldKind.Date => ((DateTimePicker)Input).Value,
        _ => ((TextBox)Input).Text
    };

    public void SetValue(object? value)
    {
        switch (Kind)
        {
            case FieldKind.Int:
            case FieldKind.Decimal:
                var num = (NumericUpDown)Input;
                var d = value is null ? 0m : Convert.ToDecimal(value);
                num.Value = Math.Clamp(d, num.Minimum, num.Maximum);
                break;
            case FieldKind.Date:
                var picker = (DateTimePicker)Input;
                var date = value is DateTime dt ? dt : DateTime.Today;
                picker.Value = date < picker.MinDate ? picker.MinDate
                    : date > picker.MaxDate ? picker.MaxDate : date;
                break;
            default:
                ((TextBox)Input).Text = value?.ToString() ?? string.Empty;
                break;
        }
    }

    public void Clear()
    {
        switch (Kind)
        {
            case FieldKind.Int:
            case FieldKind.Decimal:
                ((NumericUpDown)Input).Value = ((NumericUpDown)Input).Minimum;
                break;
            case FieldKind.Date:
                ((DateTimePicker)Input).Value = DateTime.Today;
                break;
            default:
                ((TextBox)Input).Text = string.Empty;
                break;
        }
    }
}
