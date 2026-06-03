namespace WinForms;

// Simple modal dialog to collect the fields needed by /api/Auth/register.
public class RegisterForm : Form
{
    private readonly TextBox _firstName = new();
    private readonly TextBox _lastName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly ComboBox _role = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public string FirstNameValue => _firstName.Text.Trim();
    public string LastNameValue => _lastName.Text.Trim();
    public string EmailValue => _email.Text.Trim();
    public string PasswordValue => _password.Text;
    public string RoleValue => _role.SelectedItem?.ToString() ?? "Guest";

    public RegisterForm()
    {
        Text = "Register";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 250);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(12),
            RowCount = 6
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        foreach (var box in new[] { _firstName, _lastName, _email, _password })
            box.Dock = DockStyle.Fill;
        _role.Dock = DockStyle.Fill;
        _role.Items.AddRange(new object[] { "Guest", "Manager", "Admin" });
        _role.SelectedIndex = 0;

        layout.Controls.Add(new Label { Text = "First name", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_firstName, 1, 0);
        layout.Controls.Add(new Label { Text = "Last name", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_lastName, 1, 1);
        layout.Controls.Add(new Label { Text = "Email", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        layout.Controls.Add(_email, 1, 2);
        layout.Controls.Add(new Label { Text = "Password", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(_password, 1, 3);
        layout.Controls.Add(new Label { Text = "Role", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        layout.Controls.Add(_role, 1, 4);

        var ok = new Button { Text = "Register", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.AddRange(new Control[] { cancel, ok });
        layout.Controls.Add(buttons, 1, 5);

        Controls.Add(layout);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
