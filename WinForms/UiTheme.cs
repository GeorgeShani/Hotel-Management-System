namespace WinForms;

// Central place for colors, fonts and control styling so the whole app
// shares one consistent, modern-ish look.
public static class UiTheme
{
    public static readonly Color Primary = Color.FromArgb(45, 125, 210);
    public static readonly Color PrimaryDark = Color.FromArgb(31, 42, 68);
    public static readonly Color Sidebar = Color.FromArgb(33, 40, 56);
    public static readonly Color SidebarActive = Color.FromArgb(45, 125, 210);
    public static readonly Color SidebarHover = Color.FromArgb(48, 58, 80);
    public static readonly Color Accent = Color.FromArgb(46, 168, 116);
    public static readonly Color Danger = Color.FromArgb(206, 73, 73);
    public static readonly Color Bg = Color.FromArgb(244, 246, 249);
    public static readonly Color Card = Color.White;
    public static readonly Color Border = Color.FromArgb(223, 227, 233);
    public static readonly Color TextDark = Color.FromArgb(40, 44, 52);
    public static readonly Color TextMuted = Color.FromArgb(120, 128, 140);

    public static readonly Font Title = new("Segoe UI Semibold", 15f);
    public static readonly Font Heading = new("Segoe UI Semibold", 12f);
    public static readonly Font Base = new("Segoe UI", 9.75f);
    public static readonly Font Small = new("Segoe UI", 8.5f);

    public static void StyleButton(Button b, Color back, Color? fore = null)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = back;
        b.ForeColor = fore ?? Color.White;
        b.Font = Base;
        b.Cursor = Cursors.Hand;
        // Auto-size to the text so labels are never clipped; padding gives breathing room.
        b.AutoSize = true;
        b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        b.MinimumSize = new Size(80, 34);
        b.Padding = new Padding(14, 6, 14, 6);
        b.Margin = new Padding(4, 0, 4, 0);
        b.TextAlign = ContentAlignment.MiddleCenter;
        b.UseCompatibleTextRendering = false;
        b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.15f);
        b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back, 0.05f);
    }

    public static void StyleGrid(DataGridView g)
    {
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Card;
        g.Font = Base;
        g.EnableHeadersVisualStyles = false;
        g.RowHeadersVisible = false;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.ReadOnly = true;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.GridColor = Border;
        g.ColumnHeadersHeight = 38;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersDefaultCellStyle.BackColor = PrimaryDark;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.75f);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 231, 250);
        g.DefaultCellStyle.SelectionForeColor = TextDark;
        g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        g.RowTemplate.Height = 30;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }

    // A simple white "card" container with a subtle border.
    public static Panel Card2(DockStyle dock)
        => new() { Dock = dock, BackColor = Card, Padding = new Padding(1), BorderStyle = BorderStyle.FixedSingle };
}
