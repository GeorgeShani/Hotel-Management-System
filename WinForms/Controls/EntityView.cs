using System.ComponentModel;

namespace WinForms.Controls;

// Lets the host refresh a view without knowing its generic type argument.
public interface IRefreshable
{
    Task RefreshAsync();
}

// Lets the host enable/disable the create/update/delete actions (read stays open).
public interface IEntityView : IRefreshable
{
    void SetWriteEnabled(bool enabled, string? reason = null);
}

// A full resource screen: a styled list (grid) on top and a labeled-field form
// below. Clicking a row loads its values into the form; "New" clears it.
// "Save" creates (Id == 0) or updates; "Delete" removes the selected row.
public class EntityView<T> : UserControl, IEntityView where T : class, new()
{
    private readonly DataGridView _grid = new();
    private readonly FlowLayoutPanel _fieldsPanel = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 8, 12, 8) };
    private readonly Label _editingLabel = new() { AutoSize = true, ForeColor = UiTheme.TextMuted, Font = UiTheme.Base };
    private readonly Button _btnRefresh = new() { Text = "⟳ Refresh" };
    private readonly Button _btnNew = new() { Text = "+ New" };
    private readonly Button _btnSave = new() { Text = "Save" };
    private readonly Button _btnDelete = new() { Text = "Delete" };
    private readonly Label _lockNote = new() { AutoSize = true, ForeColor = UiTheme.Danger, Font = UiTheme.Base, Visible = false };
    private readonly FormField[] _fields;
    private bool _canWrite = true;

    public FlowLayoutPanel Toolbar { get; } = new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 6, 8, 6), BackColor = UiTheme.Card };

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<Task<List<T>>>? LoadList { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<T, Task>? CreateItem { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<T, Task>? UpdateItem { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<T, Task>? DeleteItem { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<T, int>? GetId { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<T>? NewItem { get; set; }

    public EntityView(string title, params FormField[] fields)
    {
        _fields = fields;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Padding = new Padding(16);

        Controls.Add(BuildGridCard());     // fill (added first so it fills remaining space)
        Controls.Add(BuildTitle(title));   // top
        Controls.Add(BuildEditorCard());   // bottom
    }

    // ---- Title -----------------------------------------------------------
    private Control BuildTitle(string title)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = UiTheme.Bg };
        panel.Controls.Add(new Label
        {
            Text = title,
            Font = UiTheme.Title,
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(2, 6)
        });
        return panel;
    }

    // ---- Grid + toolbar --------------------------------------------------
    private Control BuildGridCard()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Card, Padding = new Padding(1), Margin = new Padding(0, 0, 0, 8) };
        card.BorderStyle = BorderStyle.FixedSingle;

        UiTheme.StyleGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.SelectionChanged += (_, _) => LoadSelectedIntoForm();

        UiTheme.StyleButton(_btnRefresh, UiTheme.Primary);
        _btnRefresh.Click += async (_, _) => await RefreshAsync();
        Toolbar.Controls.Add(_btnRefresh);

        card.Controls.Add(_grid);
        card.Controls.Add(Toolbar);
        return card;
    }

    // ---- Editor form -----------------------------------------------------
    private Control BuildEditorCard()
    {
        var card = new Panel { Dock = DockStyle.Bottom, Height = 230, BackColor = UiTheme.Card, Padding = new Padding(1) };
        card.BorderStyle = BorderStyle.FixedSingle;

        var header = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = UiTheme.Card, Padding = new Padding(12, 8, 12, 0) };
        header.Controls.Add(new Label { Text = "Details", Font = UiTheme.Heading, ForeColor = UiTheme.TextDark, AutoSize = true, Location = new Point(12, 8) });
        _editingLabel.Location = new Point(90, 11);
        header.Controls.Add(_editingLabel);

        foreach (var f in _fields)
        {
            var holder = new Panel { Width = 224, Height = 52, Margin = new Padding(4) };
            holder.Controls.Add(new Label { Text = f.Label, AutoSize = true, ForeColor = UiTheme.TextMuted, Font = UiTheme.Small, Location = new Point(2, 0) });
            var input = f.Build();
            input.Location = new Point(2, 20);
            input.Width = 214;
            holder.Controls.Add(input);
            _fieldsPanel.Controls.Add(holder);
        }

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(12, 7, 12, 7), BackColor = UiTheme.Card };
        UiTheme.StyleButton(_btnNew, UiTheme.Accent);
        UiTheme.StyleButton(_btnSave, UiTheme.Primary);
        UiTheme.StyleButton(_btnDelete, UiTheme.Danger);
        _btnNew.Click += (_, _) => StartNew();
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnDelete.Click += async (_, _) => await DeleteAsync();
        _lockNote.Margin = new Padding(12, 8, 0, 0);
        buttons.Controls.AddRange(new Control[] { _btnNew, _btnSave, _btnDelete, _lockNote });

        card.Controls.Add(_fieldsPanel);
        card.Controls.Add(buttons);
        card.Controls.Add(header);
        return card;
    }

    // ---- Behavior --------------------------------------------------------
    private void LoadSelectedIntoForm()
    {
        if (_grid.CurrentRow?.DataBoundItem is not T item) return;
        foreach (var f in _fields)
            f.SetValue(GetProp(f.Property)?.GetValue(item));
        var id = GetId?.Invoke(item) ?? 0;
        _editingLabel.Text = id > 0 ? $"— editing record #{id}" : "— new record";
    }

    public void StartNew()
    {
        _grid.ClearSelection();
        var template = NewItem?.Invoke();
        foreach (var f in _fields)
        {
            if (template is not null) f.SetValue(GetProp(f.Property)?.GetValue(template));
            else f.Clear();
        }
        _editingLabel.Text = "— new record";
    }

    // Enables/disables create-update-delete. Reading (grid + Refresh) stays available.
    public void SetWriteEnabled(bool enabled, string? reason = null)
    {
        _canWrite = enabled;
        _btnNew.Enabled = enabled;
        _btnSave.Enabled = enabled;
        _btnDelete.Enabled = enabled;
        _btnNew.BackColor = enabled ? UiTheme.Accent : Color.Silver;
        _btnSave.BackColor = enabled ? UiTheme.Primary : Color.Silver;
        _btnDelete.BackColor = enabled ? UiTheme.Danger : Color.Silver;
        _lockNote.Text = string.IsNullOrEmpty(reason) ? string.Empty : "🔒  " + reason;
        _lockNote.Visible = !enabled && !string.IsNullOrEmpty(reason);
    }

    public async Task RefreshAsync()
    {
        if (LoadList is null) return;
        await Guard(async () =>
        {
            var items = await LoadList();
            _grid.DataSource = new BindingList<T>(items);
            StartNew();
        });
    }

    private T BuildModelFromForm()
    {
        var model = new T();
        foreach (var f in _fields)
        {
            var prop = GetProp(f.Property);
            if (prop is null || !prop.CanWrite) continue;
            var value = f.GetValue();
            prop.SetValue(model, value is null ? null : Convert.ChangeType(value, prop.PropertyType));
        }
        return model;
    }

    private async Task SaveAsync()
    {
        if (!_canWrite) return;
        await Guard(async () =>
        {
            var model = BuildModelFromForm();
            var id = GetId?.Invoke(model) ?? 0;
            if (id == 0)
            {
                if (CreateItem is null) { Info("Creating is not supported here."); return; }
                await CreateItem(model);
            }
            else
            {
                if (UpdateItem is null) { Info("Updating is not supported here."); return; }
                await UpdateItem(model);
            }
            await RefreshAsync();
        });
    }

    private async Task DeleteAsync()
    {
        if (!_canWrite) return;
        var model = BuildModelFromForm();
        var id = GetId?.Invoke(model) ?? 0;
        if (id == 0) { Info("Select a saved row to delete."); return; }
        if (DeleteItem is null) { Info("Deleting is not supported here."); return; }
        if (MessageBox.Show($"Delete record #{id}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        await Guard(async () =>
        {
            await DeleteItem(model);
            await RefreshAsync();
        });
    }

    private static System.Reflection.PropertyInfo? GetProp(string name) => typeof(T).GetProperty(name);

    private static async Task Guard(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static void Info(string text) => MessageBox.Show(text, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
