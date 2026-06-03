using System.Text;
using System.Text.Json;
using WinForms.Controls;
using WinForms.Models;
using WinForms.Services;

namespace WinForms;

public partial class MainForm : Form
{
    private const string DefaultBaseUrl = "https://localhost:7003";

    private readonly ApiClient _api = new(DefaultBaseUrl);

    private readonly TextBox _apiUrl = new() { Text = DefaultBaseUrl, Width = 190, BorderStyle = BorderStyle.FixedSingle };
    private readonly Label _status = new() { Text = "Not signed in", AutoSize = true, Font = UiTheme.Base, ForeColor = Color.Gainsboro };
    private readonly Button _btnLogin = new() { Text = "Sign in" };
    private readonly Button _btnRegister = new() { Text = "Register" };
    private readonly Button _btnLogout = new() { Text = "Sign out", Enabled = false };

    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Bg };
    private readonly Dictionary<string, Control> _views = new();
    private readonly List<Button> _navButtons = new();
    private string? _lastEmail;
    private string[] _roles = Array.Empty<string>();

    public MainForm()
    {
        InitializeComponent();
        Text = "Hotel Management System";
        BackColor = UiTheme.Bg;
        Font = UiTheme.Base;
        ClientSize = new Size(1180, 740);
        MinimumSize = new Size(960, 600);
        StartPosition = FormStartPosition.CenterScreen;

        BuildViews();
        Controls.Add(_content);        // fill
        Controls.Add(BuildSidebar());  // left
        Controls.Add(BuildHeader());   // top (added last so it spans full width)

        ApplyPermissions();            // start signed-out: writes locked, viewing open
        Activate("Hotels");
    }

    // ---- Header --------------------------------------------------------------
    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = UiTheme.PrimaryDark };

        header.Controls.Add(new Label
        {
            Text = "  Hotel Management System",
            Font = UiTheme.Title,
            ForeColor = Color.White,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false,
            Width = 360
        });

        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Width = 760,
            Padding = new Padding(0, 11, 12, 0)
        };

        UiTheme.StyleButton(_btnLogout, UiTheme.Danger);
        UiTheme.StyleButton(_btnRegister, UiTheme.Accent);
        UiTheme.StyleButton(_btnLogin, UiTheme.Primary);
        _btnLogin.Click += async (_, _) => await LoginAsync();
        _btnRegister.Click += async (_, _) => await RegisterAsync();
        _btnLogout.Click += (_, _) => SetSession(null);

        _apiUrl.Margin = new Padding(6, 4, 6, 0);
        _apiUrl.Leave += (_, _) => _api.SetBaseUrl(_apiUrl.Text.Trim());
        _status.Margin = new Padding(10, 8, 10, 0);

        right.Controls.Add(_btnLogout);
        right.Controls.Add(_btnRegister);
        right.Controls.Add(_btnLogin);
        right.Controls.Add(_status);
        right.Controls.Add(_apiUrl);
        right.Controls.Add(new Label { Text = "API", ForeColor = Color.Gainsboro, AutoSize = true, Margin = new Padding(6, 8, 0, 0) });

        header.Controls.Add(right);
        return header;
    }

    // ---- Sidebar navigation --------------------------------------------------
    private Control BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 186, BackColor = UiTheme.Sidebar };

        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 8, 0, 0) };
        foreach (var name in new[] { "Hotels", "Rooms", "Guests", "Managers", "Reservations", "Help" })
            nav.Controls.Add(MakeNavButton(name));

        var brand = new Label
        {
            Text = "  \U0001F3E8  HMS",
            Dock = DockStyle.Top,
            Height = 44,
            ForeColor = Color.White,
            Font = UiTheme.Heading,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.PrimaryDark
        };

        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(brand);
        return sidebar;
    }

    private Button MakeNavButton(string name)
    {
        var b = new Button
        {
            Text = "   " + name,
            Tag = name,
            Width = 186,
            Height = 46,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.Gainsboro,
            BackColor = UiTheme.Sidebar,
            Font = UiTheme.Base,
            Cursor = Cursors.Hand,
            Margin = new Padding(0)
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover;
        b.Click += (_, _) => Activate(name);
        _navButtons.Add(b);
        return b;
    }

    private void Activate(string name)
    {
        foreach (var b in _navButtons)
        {
            var active = (string)b.Tag! == name;
            b.BackColor = active ? UiTheme.SidebarActive : UiTheme.Sidebar;
            b.ForeColor = active ? Color.White : Color.Gainsboro;
            b.Font = active ? new Font(UiTheme.Base, FontStyle.Bold) : UiTheme.Base;
        }

        foreach (var v in _views.Values) v.Visible = false;
        if (_views.TryGetValue(name, out var view))
        {
            view.Visible = true;
            view.BringToFront();
            if (view is IRefreshable r) _ = r.RefreshAsync();
        }
    }

    // ---- Authentication ------------------------------------------------------
    private async Task LoginAsync()
    {
        using var dialog = new LoginForm(_lastEmail);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _api.SetBaseUrl(_apiUrl.Text.Trim());
            var result = await _api.LoginAsync(dialog.EmailValue, dialog.PasswordValue);
            if (!result.IsSuccess) { Warn(result.Message, "Sign-in failed"); return; }
            _lastEmail = dialog.EmailValue;
            SetSession(result.Token);
            RefreshActiveView();
        }
        catch (Exception ex) { Error(ex.Message); }
    }

    private async Task RegisterAsync()
    {
        using var dialog = new RegisterForm();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _api.SetBaseUrl(_apiUrl.Text.Trim());
            var result = await _api.RegisterAsync(dialog.FirstNameValue, dialog.LastNameValue,
                dialog.EmailValue, dialog.PasswordValue, dialog.RoleValue);
            if (!result.IsSuccess) { Warn(result.Message, "Registration failed"); return; }
            _lastEmail = dialog.EmailValue;
            SetSession(result.Token);
            RefreshActiveView();
            Info("Registered and signed in.");
        }
        catch (Exception ex) { Error(ex.Message); }
    }

    private void SetSession(string? token)
    {
        _api.SetToken(token);
        var on = !string.IsNullOrEmpty(token);
        _btnLogout.Enabled = on;
        _btnLogin.Enabled = !on;
        _roles = on ? ReadRoles(token!) : Array.Empty<string>();
        if (on)
        {
            _status.Text = "Signed in" + (_roles.Length > 0 ? $" · {string.Join(", ", _roles)}" : "");
            _status.ForeColor = Color.PaleGreen;
        }
        else
        {
            _status.Text = "Not signed in";
            _status.ForeColor = Color.Gainsboro;
        }
        ApplyPermissions();
    }

    private bool SignedIn => _api.IsAuthenticated;
    private bool HasRole(params string[] roles) => roles.Any(r => _roles.Contains(r, StringComparer.OrdinalIgnoreCase));

    // Mirrors the BackEnd authorization rules in the UI: viewing is open for
    // Hotels/Rooms, but creating/editing/deleting needs an authorized role.
    private void ApplyPermissions()
    {
        Set("Hotels", HasRole("Admin"), "Sign in as Admin to add, edit or delete hotels.");
        Set("Rooms", HasRole("Admin", "Manager"), "Sign in as Admin or Manager to add, edit or delete rooms.");
        Set("Managers", HasRole("Admin"), "Sign in as Admin to manage managers.");
        Set("Reservations", SignedIn, "Sign in to add, edit or delete reservations.");

        // Guest records are off-limits to the Guest role entirely – the API rejects
        // even reading them – so the whole tab is hidden unless you're Admin/Manager.
        var canSeeGuests = HasRole("Admin", "Manager");
        Set("Guests", canSeeGuests, "Sign in as Admin or Manager to manage guests.");
        SetTabVisible("Guests", canSeeGuests);

        void Set(string name, bool enabled, string reason)
        {
            if (_views.TryGetValue(name, out var v) && v is IEntityView ev)
                ev.SetWriteEnabled(enabled, enabled ? null : reason);
        }

        void SetTabVisible(string name, bool visible)
        {
            var btn = _navButtons.FirstOrDefault(b => (string)b.Tag! == name);
            if (btn != null) btn.Visible = visible;
            // If the active tab just got hidden, fall back to a safe default.
            if (!visible && _views.TryGetValue(name, out var v) && v.Visible)
                Activate("Hotels");
        }
    }

    private void RefreshActiveView()
    {
        foreach (var v in _views.Values)
            if (v.Visible && v is IRefreshable r) _ = r.RefreshAsync();
    }

    private static string[] ReadRoles(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return Array.Empty<string>();
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            foreach (var name in new[] { "role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" })
            {
                if (!doc.RootElement.TryGetProperty(name, out var el)) continue;
                return el.ValueKind == JsonValueKind.Array
                    ? el.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s.Length > 0).ToArray()
                    : new[] { el.GetString() ?? "" };
            }
        }
        catch { /* display only */ }
        return Array.Empty<string>();
    }

    // ---- Views ---------------------------------------------------------------
    private void BuildViews()
    {
        Add("Hotels", BuildHotels());
        Add("Rooms", BuildRooms());
        Add("Guests", BuildGuests());
        Add("Managers", BuildManagers());
        Add("Reservations", BuildReservations());
        Add("Help", BuildHelp());
    }

    private Control BuildHelp()
    {
        var root = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Bg, Padding = new Padding(16) };

        var title = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = UiTheme.Bg };
        title.Controls.Add(new Label { Text = "How to use this system", Font = UiTheme.Title, ForeColor = UiTheme.TextDark, AutoSize = true, Location = new Point(2, 6) });

        var card = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Card, Padding = new Padding(20), BorderStyle = BorderStyle.FixedSingle };
        var rtb = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Card,
            Font = UiTheme.Base,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            TabStop = false
        };

        void Heading(string s)
        {
            rtb.SelectionFont = new Font("Segoe UI Semibold", 12f);
            rtb.SelectionColor = UiTheme.Primary;
            rtb.AppendText(s + "\n");
        }
        void Para(string s)
        {
            rtb.SelectionFont = UiTheme.Base;
            rtb.SelectionColor = UiTheme.TextDark;
            rtb.AppendText(s + "\n");
        }
        void Bullet(string s)
        {
            rtb.SelectionFont = UiTheme.Base;
            rtb.SelectionColor = UiTheme.TextDark;
            rtb.AppendText("   •  " + s + "\n");
        }
        void Gap() => rtb.AppendText("\n");

        Heading("Getting started");
        Bullet("Make sure the BackEnd API is running, then check the 'API' box in the top-right points to it (default https://localhost:7003).");
        Bullet("Click 'Register' to create an account, or 'Sign in' if you already have one. Your role is shown next to the buttons once signed in.");
        Bullet("Browsing Hotels and Rooms works without signing in; everything else requires a signed-in account.");
        Gap();

        Heading("Roles & permissions");
        Bullet("Admin  – full access: manage Hotels, Rooms, Managers, Guests and Reservations.");
        Bullet("Manager – can manage Rooms and Guests (plus Reservations).");
        Bullet("Guest  – can create and manage only their own Reservations; cannot access Guest records.");
        Para("   If an action is blocked, the server returns an authorization error and it appears in a popup. Sign in with an account that has the right role.");
        Gap();

        Heading("Working with a list (every tab works the same)");
        Bullet("Use the left sidebar to switch between tabs. Some tabs appear only for the roles allowed to use them.");
        Bullet("The grid at the top shows existing records. Click 'Refresh' to reload from the server.");
        Bullet("Click any row – its values load into the form below and it shows '— editing record #N'.");
        Bullet("Edit the fields and click 'Save' to update that record.");
        Bullet("Click '+ New' to clear the form (shows '— new record'); fill it in and 'Save' to create.");
        Bullet("Click 'Delete' to remove the record currently loaded in the form (you'll be asked to confirm).");
        Gap();

        Heading("Tab-specific notes");
        Bullet("Rooms: rooms belong to a hotel. Set 'Show rooms for Hotel Id' at the top to list a hotel's rooms, and set 'Hotel Id' in the form when creating a room.");
        Bullet("Managers: a manager is assigned to a hotel via 'Hotel Id'. Email and personal number are set only at creation.");
        Bullet("Guests / Managers: 'Personal number' must be unique and cannot be changed after creation.");
        Bullet("Reservations: enter 'Guest Id' and one or more 'Room ids' as a comma-separated list (e.g. 1,2). Pick check-in / check-out dates; the 'Total price' is calculated by the server and shown after saving.");
        Bullet("Reservations are owned by their creator: as a Guest you only see and manage the reservations you made, while Admins and Managers see all of them.");
        Gap();

        Heading("Troubleshooting");
        Bullet("'Could not reach the server' – the API isn't running, or the 'API' URL is wrong.");
        Bullet("Certificate warnings – trust the local dev certificate once with:  dotnet dev-certs https --trust");
        Bullet("401 / 403 errors – you're not signed in, or your role can't perform that action.");
        Bullet("Validation errors (e.g. duplicate personal number, invalid dates) are shown exactly as the server reports them.");

        rtb.SelectionStart = 0;
        rtb.ScrollToCaret();

        card.Controls.Add(rtb);
        root.Controls.Add(card);
        root.Controls.Add(title);
        return root;
    }

    private void Add(string name, Control view)
    {
        view.Visible = false;
        _views[name] = view;
        _content.Controls.Add(view);
    }

    private EntityView<HotelModel> BuildHotels()
    {
        return new EntityView<HotelModel>("Hotels",
            new FormField(nameof(HotelModel.Id), "Id", FieldKind.Int, readOnly: true),
            new FormField(nameof(HotelModel.Name), "Name", FieldKind.Text),
            new FormField(nameof(HotelModel.Rating), "Rating (1-5)", FieldKind.Int),
            new FormField(nameof(HotelModel.Country), "Country", FieldKind.Text),
            new FormField(nameof(HotelModel.City), "City", FieldKind.Text),
            new FormField(nameof(HotelModel.Address), "Address", FieldKind.Text))
        {
            GetId = h => h.Id,
            LoadList = () => _api.GetListAsync<HotelModel>("api/Hotels"),
            CreateItem = h => _api.PostAsync("api/Hotels", new { h.Name, h.Rating, h.Country, h.City, h.Address }),
            UpdateItem = h => _api.PutAsync($"api/Hotels/{h.Id}", new { h.Name, h.Rating, h.Country, h.City, h.Address }),
            DeleteItem = h => _api.DeleteAsync($"api/Hotels/{h.Id}")
        };
    }

    private EntityView<RoomModel> BuildRooms()
    {
        var hotelId = new NumericUpDown { Minimum = 1, Maximum = 1_000_000, Value = 1, Width = 80, Font = UiTheme.Base };
        var view = new EntityView<RoomModel>("Rooms",
            new FormField(nameof(RoomModel.Id), "Id", FieldKind.Int, readOnly: true),
            new FormField(nameof(RoomModel.Name), "Name", FieldKind.Text),
            new FormField(nameof(RoomModel.Price), "Price", FieldKind.Decimal),
            new FormField(nameof(RoomModel.HotelId), "Hotel Id", FieldKind.Int))
        {
            GetId = r => r.Id,
            NewItem = () => new RoomModel { HotelId = (int)hotelId.Value },
            LoadList = () => _api.GetListAsync<RoomModel>($"api/Rooms/hotel/{(int)hotelId.Value}"),
            CreateItem = r => _api.PostAsync("api/Rooms", new { r.Name, r.Price, r.HotelId }),
            UpdateItem = r => _api.PutAsync($"api/Rooms/{r.Id}", new { r.Name, r.Price }),
            DeleteItem = r => _api.DeleteAsync($"api/Rooms/{r.Id}")
        };
        view.Toolbar.Controls.Add(new Label { Text = "Show rooms for Hotel Id:", AutoSize = true, ForeColor = UiTheme.TextMuted, Margin = new Padding(14, 9, 4, 0) });
        view.Toolbar.Controls.Add(hotelId);
        return view;
    }

    private EntityView<GuestModel> BuildGuests()
    {
        return new EntityView<GuestModel>("Guests",
            new FormField(nameof(GuestModel.Id), "Id", FieldKind.Int, readOnly: true),
            new FormField(nameof(GuestModel.FirstName), "First name", FieldKind.Text),
            new FormField(nameof(GuestModel.LastName), "Last name", FieldKind.Text),
            new FormField(nameof(GuestModel.PersonalNumber), "Personal number", FieldKind.Text),
            new FormField(nameof(GuestModel.PhoneNumber), "Phone", FieldKind.Text))
        {
            GetId = g => g.Id,
            LoadList = () => _api.GetListAsync<GuestModel>("api/Guests"),
            CreateItem = g => _api.PostAsync("api/Guests", new { g.FirstName, g.LastName, g.PersonalNumber, g.PhoneNumber }),
            UpdateItem = g => _api.PutAsync($"api/Guests/{g.Id}", new { g.FirstName, g.LastName, g.PhoneNumber }),
            DeleteItem = g => _api.DeleteAsync($"api/Guests/{g.Id}")
        };
    }

    private EntityView<ManagerModel> BuildManagers()
    {
        return new EntityView<ManagerModel>("Managers",
            new FormField(nameof(ManagerModel.Id), "Id", FieldKind.Int, readOnly: true),
            new FormField(nameof(ManagerModel.FirstName), "First name", FieldKind.Text),
            new FormField(nameof(ManagerModel.LastName), "Last name", FieldKind.Text),
            new FormField(nameof(ManagerModel.PersonalNumber), "Personal number", FieldKind.Text),
            new FormField(nameof(ManagerModel.Email), "Email", FieldKind.Text),
            new FormField(nameof(ManagerModel.PhoneNumber), "Phone", FieldKind.Text),
            new FormField(nameof(ManagerModel.HotelId), "Hotel Id", FieldKind.Int))
        {
            GetId = m => m.Id,
            LoadList = () => _api.GetListAsync<ManagerModel>("api/Managers"),
            CreateItem = m => _api.PostAsync("api/Managers", new { m.FirstName, m.LastName, m.PersonalNumber, m.Email, m.PhoneNumber, m.HotelId }),
            UpdateItem = m => _api.PutAsync($"api/Managers/{m.Id}", new { m.FirstName, m.LastName, m.PhoneNumber }),
            DeleteItem = m => _api.DeleteAsync($"api/Managers/{m.Id}")
        };
    }

    private EntityView<ReservationModel> BuildReservations()
    {
        return new EntityView<ReservationModel>("Reservations",
            new FormField(nameof(ReservationModel.Id), "Id", FieldKind.Int, readOnly: true),
            new FormField(nameof(ReservationModel.GuestId), "Guest Id", FieldKind.Int),
            new FormField(nameof(ReservationModel.RoomIds), "Room ids (e.g. 1,2)", FieldKind.Text),
            new FormField(nameof(ReservationModel.CheckInDate), "Check-in", FieldKind.Date),
            new FormField(nameof(ReservationModel.CheckOutDate), "Check-out", FieldKind.Date),
            new FormField(nameof(ReservationModel.TotalPrice), "Total price", FieldKind.Decimal, readOnly: true))
        {
            GetId = r => r.Id,
            NewItem = () => new ReservationModel(),
            LoadList = async () =>
            {
                var dtos = await _api.GetListAsync<ReservationDtoJson>("api/Reservations");
                return dtos.Select(d => new ReservationModel
                {
                    Id = d.Id,
                    GuestId = d.GuestId,
                    RoomIds = string.Join(",", d.RoomIds),
                    CheckInDate = d.CheckInDate,
                    CheckOutDate = d.CheckOutDate,
                    TotalPrice = d.TotalPrice
                }).ToList();
            },
            CreateItem = r => _api.PostAsync("api/Reservations", new { r.GuestId, RoomIds = ParseIds(r.RoomIds), r.CheckInDate, r.CheckOutDate }),
            UpdateItem = r => _api.PutAsync($"api/Reservations/{r.Id}", new { r.CheckInDate, r.CheckOutDate }),
            DeleteItem = r => _api.DeleteAsync($"api/Reservations/{r.Id}")
        };
    }

    private static List<int> ParseIds(string csv) =>
        (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : -1)
            .Where(n => n > 0)
            .ToList();

    // ---- Small helpers -------------------------------------------------------
    private static void Info(string text) => MessageBox.Show(text, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    private static void Warn(string text, string title) => MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    private static void Error(string text) => MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
