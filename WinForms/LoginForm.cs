namespace WinForms;

// Simple modal dialog to collect credentials for /api/Auth/login.
public class LoginForm : Form
{
    private readonly TextBox _email = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };

    public string EmailValue => _email.Text.Trim();
    public string PasswordValue => _password.Text;

    public LoginForm(string? prefillEmail = null)
    {
        Text = "Sign in";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 170);
        BackColor = UiTheme.Card;
        Font = UiTheme.Base;
        _email.Text = prefillEmail ?? string.Empty;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(14) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _email.Dock = DockStyle.Fill;
        _password.Dock = DockStyle.Fill;

        layout.Controls.Add(new Label { Text = "Email", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_email, 1, 0);
        layout.Controls.Add(new Label { Text = "Password", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_password, 1, 1);

        var ok = new Button { Text = "Sign in", DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        UiTheme.StyleButton(ok, UiTheme.Primary);
        UiTheme.StyleButton(cancel, Color.Gray);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.AddRange(new Control[] { cancel, ok });
        layout.Controls.Add(buttons, 1, 2);

        Controls.Add(layout);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
