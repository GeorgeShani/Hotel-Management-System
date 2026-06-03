# Hotel Management System

A small hotel-management solution made up of two projects:

| Project       | Type                         | Target              | Purpose                                            |
|---------------|------------------------------|---------------------|----------------------------------------------------|
| **BackEnd**   | ASP.NET Core Web API         | `net10.0`           | REST API, authentication, business logic, database |
| **WinForms**  | Windows Forms desktop client | `net10.0-windows`   | Functional UI that consumes the BackEnd API        |

```
HotelManagementSystem/
├─ HotelManagementSystem.sln
├─ BackEnd/      → REST API (controllers, services, repositories, EF Core, JWT)
└─ WinForms/     → Desktop client (login + CRUD tabs)
```

---

## Prerequisites

- **.NET 10 SDK**
- **SQL Server** (LocalDB, Express, or full) reachable from the connection string
- **Windows** (required to run the WinForms client)
- `dotnet-ef` global tool (for migrations):
  ```bash
  dotnet tool install --global dotnet-ef --version 10.0.0
  ```

---

## BackEnd

ASP.NET Core Web API built with a clean, layered structure inside a single project.

### Architecture

```
BackEnd/
├─ Controllers/    → HTTP endpoints (Auth, Hotels, Rooms, Guests, Managers, Reservations)
├─ Services/       → Business logic + validation
├─ Repositories/   → Data access (Generic + per-entity repositories)
├─ Interfaces/     → Service & repository abstractions
├─ DTOs/           → Request/response contracts (per resource)
├─ Models/         → EF Core entities + ApplicationUser (Identity)
├─ Data/           → AppDbContext + EF migrations
├─ Mappings/       → AutoMapper profile
├─ Middlewares/    → Global exception handling
└─ Program.cs      → DI registration, Identity, JWT, Swagger, pipeline
```

### Key technologies

- Entity Framework Core 10 (SQL Server, code-first migrations)
- ASP.NET Core Identity (`ApplicationUser : IdentityUser`, role-based)
- JWT Bearer authentication
- AutoMapper (profile-based mapping)
- Swagger / Swashbuckle (with Bearer auth support)
- Repository + Service pattern

### Configuration — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HMS_FinalProjectDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "ThisIsMySuperSecretKeyForHotelManagementSystem2026!",
    "Issuer": "HMSApi",
    "Audience": "HMSApiUsers",
    "DurationInMinutes": 60
  }
}
```

- **`DefaultConnection`** — update `Server`/`Database` to match your SQL Server instance.
- **`Jwt:Key`** — symmetric signing key. For real deployments move this out of source control (user-secrets / environment variables) and use a long random value.

### Database setup

From the `BackEnd` folder:

```bash
# Apply the existing migration to a fresh database
dotnet ef database update
```

To start migrations from scratch instead:

```bash
# delete BackEnd/Data/Migrations first, then:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Running

```bash
cd BackEnd
dotnet run
```

By default the API listens on:

- `https://localhost:7003`
- `http://localhost:5126`

Swagger UI opens automatically in the browser at `/swagger` (Development environment).

> **HTTPS dev certificate:** if the browser warns about the certificate, trust it once with
> `dotnet dev-certs https --trust`.

### API endpoints

Base route: `api/[controller]`. Roles: **Admin**, **Manager**, **Guest**.

#### Auth — `api/Auth` (anonymous)
| Method | Route             | Body          | Description                          |
|--------|-------------------|---------------|--------------------------------------|
| POST   | `/register`       | `RegisterDto` | Create a user (default role `Guest`) |
| POST   | `/login`          | `LoginDto`    | Returns a JWT token                  |

#### Hotels — `api/Hotels`
| Method | Route                          | Auth   | Description                         |
|--------|--------------------------------|--------|-------------------------------------|
| GET    | `/?country=&city=&rating=`     | anon   | List hotels (optional filters)      |
| GET    | `/{id}`                        | anon   | Get a hotel                         |
| POST   | `/`                            | Admin  | Create a hotel                      |
| PUT    | `/{id}`                        | Admin  | Update a hotel                      |
| DELETE | `/{id}`                        | Admin  | Delete a hotel                      |

#### Rooms — `api/Rooms`
| Method | Route                  | Auth           | Description                  |
|--------|------------------------|----------------|------------------------------|
| GET    | `/hotel/{hotelId}`     | anon           | List rooms of a hotel        |
| GET    | `/{id}`                | anon           | Get a room                   |
| POST   | `/`                    | Admin, Manager | Create a room                |
| PUT    | `/{id}`                | Admin, Manager | Update a room                |
| DELETE | `/{id}`                | Admin, Manager | Delete a room                |

#### Guests — `api/Guests` (Admin, Manager only)
| Method | Route     | Description       |
|--------|-----------|-------------------|
| GET    | `/`       | List guests       |
| GET    | `/{id}`   | Get a guest       |
| POST   | `/`       | Create a guest    |
| PUT    | `/{id}`   | Update a guest    |
| DELETE | `/{id}`   | Delete a guest    |

> The **Guest** role has no access to this controller at all — guests can never view or
> manage other guests' records.

#### Managers — `api/Managers` (Admin only)
| Method | Route     | Description        |
|--------|-----------|--------------------|
| GET    | `/`       | List managers      |
| GET    | `/{id}`   | Get a manager      |
| POST   | `/`       | Create a manager   |
| PUT    | `/{id}`   | Update a manager   |
| DELETE | `/{id}`   | Delete a manager   |

#### Reservations — `api/Reservations` (authenticated)
| Method | Route     | Auth   | Description                                          |
|--------|-----------|--------|------------------------------------------------------|
| GET    | `/`       | auth   | List reservations (Guests see only their own)        |
| GET    | `/{id}`   | auth   | Get a reservation (Guests only if they own it)       |
| POST   | `/`       | Guest  | Create a reservation (price is calculated)           |
| PUT    | `/{id}`   | auth   | Update reservation dates (Guests only their own)     |
| DELETE | `/{id}`   | auth   | Delete a reservation (Guests only their own)         |

> **Ownership:** each reservation records the Identity id of the user who created it
> (`ApplicationUserId`). **Admin** and **Manager** see and manage every reservation; a
> **Guest** is restricted to the reservations they created — attempting to read, edit or
> delete someone else's returns **403 Forbidden**.

### Authentication flow

1. `POST /api/Auth/register` or `/login` → receive `{ token, message, isSuccess }`.
2. Send the token on protected calls via the header:
   `Authorization: Bearer <token>`.
3. In Swagger, click **Authorize** and paste `Bearer <token>`.

### Error handling

A global exception middleware converts exceptions into a consistent JSON response:

```json
{ "statusCode": 400, "message": "..." }
```

- `400` — invalid operation / bad argument (validation failures)
- `403` — forbidden (e.g. a Guest accessing a reservation they don't own)
- `404` — “not found” errors
- `500` — unexpected errors

---

## WinForms (Desktop Client)

A minimalistic but functional desktop UI that exercises most BackEnd endpoints.

### Structure

```
WinForms/
├─ Program.cs               → app entry point
├─ MainForm.cs             → header bar + sidebar navigation + per-resource views (built in code)
├─ LoginForm.cs            → modal sign-in dialog
├─ RegisterForm.cs         → modal dialog for user registration (with role selector)
├─ UiTheme.cs              → shared colors, fonts and control styling helpers
├─ Models/ApiModels.cs     → client view-models (Hotel, Room, Guest, Manager, Reservation, Auth)
├─ Services/ApiClient.cs   → HttpClient wrapper: JWT handling + typed REST calls
└─ Controls/
   ├─ EntityView.cs        → reusable master/detail CRUD view (grid + labeled form + buttons)
   └─ FormField.cs         → typed input field (text / int / decimal / date)
```

### How it works

- **Header bar** — set the API base URL, then **Sign in** or **Register**. On success the JWT is
  stored and attached to every subsequent request; the status label shows the signed-in roles.
- **Sidebar** — one entry per resource: **Hotels, Rooms, Guests, Managers, Reservations**, plus a
  **Help** tab with in-app usage instructions. Tabs that a role may not use are hidden.
- **Each resource view** uses the same pattern:
  - a **grid** at the top lists items (`Refresh` to reload from the server),
  - clicking a row loads its values into a **labeled form** below (showing “— editing record #N”),
  - **+ New / Save / Delete** buttons. `+ New` clears the form for a new record; `Save` creates
    when `Id == 0`, otherwise updates.
- **Rooms tab** has a *Hotel Id* selector because rooms are listed per hotel
  (`GET /api/Rooms/hotel/{hotelId}`).
- **Reservations tab** edits `RoomIds` as a comma-separated string (e.g. `1,2,3`) and shows the
  server-calculated `TotalPrice`.

### Permissions in the UI

The client mirrors the API's authorization rules so blocked actions are surfaced before (or as)
the server rejects them:

- **Hotels / Rooms** — anyone can browse; creating, editing and deleting require **Admin**
  (Rooms also **Manager**). Write buttons are locked with an explanatory note otherwise.
- **Guests** — the tab is hidden unless you're signed in as **Admin** or **Manager**; the Guest
  role has no access to guest records at all.
- **Managers** — Admin only.
- **Reservations** — any signed-in user; a **Guest** sees and manages only the reservations they
  created.

### Configuration

The default API URL is `https://localhost:7003` (editable in the top bar at runtime).
The client accepts the local ASP.NET dev certificate without strict validation — this is
intended only for local development.

### Running

1. Start the BackEnd first (`cd BackEnd && dotnet run`).
2. Run the client:
   ```bash
   cd WinForms
   dotnet run
   ```
3. Register or log in, then use the tabs.

> **Permissions:** the UI reflects the API's role rules. For example, creating hotels/managers
> requires an **Admin** token, creating reservations requires a **Guest** token, and the Guests
> tab is only visible to Admin/Manager. Calls made without the right role return an authorization
> error shown in a message box.

---

## Quick start (end to end)

```bash
# 1. Configure BackEnd/appsettings.json connection string

# 2. Create the database
cd BackEnd
dotnet ef database update

# 3. Run the API
dotnet run        # https://localhost:7003 (+ Swagger)

# 4. In another terminal, run the desktop client
cd ../WinForms
dotnet run
```
