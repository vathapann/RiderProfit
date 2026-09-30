# RiderProfit

## 32998 .NET Application Development : Assignment 2 (Spring 2026).

A web app that helps food delivery riders track shifts, expenses and estimated profit across platforms.

### Problem:

Riders may work across multiple platforms and pay for costs such as fuel, tolls and maintenance. This makes it difficult to see their overall profit or compare which shifts are most worthwhile.

### Solution:

RiderProfit brings all of a rider's shifts and costs into one place and works out their real
profit, not just what the delivery apps paid. Riders log each shift and expense, choose the vehicle
they used, and the app subtracts running costs to show net profit per hour. It then uses that history,
along with weather forecasts and public holidays, to suggest which future shifts are likely to be
most worthwhile.

### Screens

| Screen | What it does |
|---|---|
| **Shifts** | Add, edit and delete shifts: platform, suburb, vehicle, start and end time, distance, earnings and tips, with validation. |
| **Expenses** | Record expenses by category and date, optionally linked to a shift. |
| **Profit Dashboard** | Net profit per hour, with interactive charts comparing platforms, days of the week and time slots. |
| **Shift Planner** | Compares possible working times using past shifts, the Open-Meteo weather forecast and public holidays, with an ML.NET profit prediction. |
| **Vehicles** | Bike, e-bike and car profiles with their running cost per km, used to work out each shift's cost. |
| **FY Summary** | Income, expenses and net profit for an Australian financial year (July–June), with CSV export. |

Each rider has their own account (ASP.NET Core Identity), and every rider sees only their own data.

### Team

| Member | Main responsibilities |
|---|---|
| Sovatha | Shifts, Vehicles, data model and EF Core, weather and holiday APIs |
| Tiwat | Profit Dashboard, FY Summary, charts and responsive layout |
| Ellie | Expenses, Shift Planner, ML.NET model, NUnit test project |

## Versions

| Tool / package | Version |
|---|---|
| .NET SDK | **9.0** (pinned by `global.json`: 9.0.100 or any newer 9.0.x) |
| Target framework | `net9.0` |
| UI | ASP.NET Core Blazor Web App (Interactive Server) |
| Database | SQLite via Entity Framework Core 9.0.18 |
| Authentication | ASP.NET Core Identity 9.0.18 |
| EF Core CLI (`dotnet-ef`) | 9.0.20 (local tool, see `dotnet-tools.json`) |
| IDE for the lab demo | Visual Studio 2022 (17.12 or later, with .NET 9) |

> Use .NET 9, not .NET 10. The lab builds this with Visual Studio 2022, and code that
> only compiles on .NET 10 gets zero marks. `global.json` enforces this automatically.

## First-time setup

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0). Check with:
   ```bash
   dotnet --list-sdks   # should include a 9.0.x version
   ```
2. Clone the repo and go into the solution folder (the one with `RiderProfit.sln`):
   ```bash
   git clone <repo-url>
   cd RiderProfit
   ```
3. Restore the local tools (installs `dotnet-ef` for this repo only):
   ```bash
   dotnet tool restore
   ```

You don't need to create the database. The app creates `RiderProfit/Data/app.db` and applies
all migrations automatically on startup.

## Running the dev server

From the solution folder:

```bash
dotnet watch --project RiderProfit
```

Then open **http://localhost:5101**.

- **Hot reload:** save a `.razor`, `.cs` or `.css` file and the browser updates automatically.
- **Restart:** press `Ctrl+R` in the terminal. If it asks to restart after an edit it can't
  hot-reload (e.g. `Program.cs`, new files), press `a` to always restart automatically.
- **Stop:** press `Ctrl+C`.

To run without hot reload:

```bash
dotnet run --project RiderProfit    # or ./run.sh
```

In Visual Studio 2022 / Rider, open `RiderProfit.sln` and run the `http` profile.

## Database changes (EF Core migrations)

After changing an entity class or `ApplicationDbContext`, create a migration from the solution folder:

```bash
dotnet ef migrations add <DescriptiveName> --project RiderProfit
```

Commit the generated files in `RiderProfit/Data/Migrations/`. Teammates get the change by pulling
and restarting the app, since migrations are applied on startup.

To start again with an empty database, stop the app and delete `RiderProfit/Data/app.db*`.

## Troubleshooting

| Problem | Fix |
|---|---|
| `address already in use` on port 5101 | Another copy of the app is running. Stop it (`Ctrl+C`) first. |
| `Could not find a part of the path '...obj\Debug/...'` in `dotnet watch` | You're on the .NET 10 SDK. Run from the solution folder so `global.json` selects .NET 9, and install a 9.0 SDK if needed. |
| `dotnet ef` not found | Run `dotnet tool restore` in the solution folder. |
| Page doesn't update after editing | Make sure you used `dotnet watch`, not `dotnet run`. Press `Ctrl+R` to restart. |
