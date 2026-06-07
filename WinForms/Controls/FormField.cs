namespace WinForms.Controls;

public enum FieldKind { Text, Int, Decimal, Date, Select, MultiSelect }

// One option for a Select / MultiSelect field: a stable Id and a display label.
public record SelectOption(int Id, string Text)
{
    public override string ToString() => Text; // used by CheckedListBox display
}

// Describes one editable field on an entity form: which model property it maps
// to, its label, the kind of input control, and whether it is read-only.
// EntityView<T> uses these to build the form and to read/write values.
public class FormField
{
    public string Property { get; }
    public string Label { get; }
    public FieldKind Kind { get; }
    public bool ReadOnly { get; }
    public int Minimum { get; }
    public Control Input { get; private set; } = null!;

    // For Select / MultiSelect fields: supplies the list of choices (loaded async).
    public Func<Task<IReadOnlyList<SelectOption>>>? OptionsProvider { get; }

    public FormField(string property, string label, FieldKind kind, bool readOnly = false, int minimum = 0)
    {
        Property = property;
        Label = label;
        Kind = kind;
        ReadOnly = readOnly;
        Minimum = minimum;
    }

    // Overload for dropdown / checklist fields backed by a remote option source.
    public FormField(string property, string label, FieldKind kind,
        Func<Task<IReadOnlyList<SelectOption>>> optionsProvider, bool readOnly = false)
        : this(property, label, kind, readOnly)
    {
        OptionsProvider = optionsProvider;
    }

    public Control Build()
    {
        Input = Kind switch
        {
            FieldKind.Int => new NumericUpDown { Minimum = Minimum, Maximum = 1_000_000, Width = 200 },
            FieldKind.Decimal => new NumericUpDown { Minimum = Minimum, Maximum = 100_000_000, DecimalPlaces = 2, Increment = 10, Width = 200 },
            FieldKind.Date => new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 200 },
            FieldKind.Select => new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, DisplayMember = "Text", ValueMember = "Id" },
            FieldKind.MultiSelect => new CheckedListBox { CheckOnClick = true, Width = 340, Height = 128, IntegralHeight = false, BorderStyle = BorderStyle.FixedSingle },
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

    // Loads (or reloads) the choices for Select / MultiSelect fields. Any failure
    // (e.g. a Guest hitting an Admin-only endpoint) leaves the control empty
    // instead of breaking the whole form refresh.
    public async Task LoadOptionsAsync()
    {
        if (OptionsProvider is null) return;
        IReadOnlyList<SelectOption> options;
        try { options = await OptionsProvider(); }
        catch { options = Array.Empty<SelectOption>(); }

        switch (Input)
        {
            case ComboBox combo:
                var keepId = combo.SelectedValue as int?;
                combo.DataSource = new List<SelectOption>(options);
                combo.SelectedIndex = -1;
                if (keepId is > 0) combo.SelectedValue = keepId.Value;
                break;
            case CheckedListBox list:
                list.Items.Clear();
                foreach (var o in options) list.Items.Add(o);
                break;
        }
    }

    public object? GetValue() => Kind switch
    {
        FieldKind.Int => (int)((NumericUpDown)Input).Value,
        FieldKind.Decimal => ((NumericUpDown)Input).Value,
        FieldKind.Date => ((DateTimePicker)Input).Value,
        FieldKind.Select => ((ComboBox)Input).SelectedValue as int? ?? 0,
        FieldKind.MultiSelect => ((CheckedListBox)Input).CheckedItems
            .Cast<SelectOption>().Select(o => o.Id).ToList(),
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
            case FieldKind.Select:
                var combo = (ComboBox)Input;
                var id = value is null ? 0 : Convert.ToInt32(value);
                if (id > 0) combo.SelectedValue = id; else combo.SelectedIndex = -1;
                break;
            case FieldKind.MultiSelect:
                var clb = (CheckedListBox)Input;
                var ids = (value as IEnumerable<int>)?.ToHashSet() ?? new HashSet<int>();
                for (int i = 0; i < clb.Items.Count; i++)
                    clb.SetItemChecked(i, clb.Items[i] is SelectOption o && ids.Contains(o.Id));
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
            case FieldKind.Select:
                ((ComboBox)Input).SelectedIndex = -1;
                break;
            case FieldKind.MultiSelect:
                var clb = (CheckedListBox)Input;
                for (int i = 0; i < clb.Items.Count; i++) clb.SetItemChecked(i, false);
                break;
            default:
                ((TextBox)Input).Text = string.Empty;
                break;
        }
    }
}
