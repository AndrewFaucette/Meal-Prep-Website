# MealPrep API

This is the ASP.NET Core backend for the meal-prep application.

## Authentication endpoints

ASP.NET Core Identity exposes these routes under `/api/auth`:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/auth/me`

The API uses bearer tokens returned by the login endpoint. Send the access token on protected requests with:

```text
Authorization: Bearer <access-token>
```

## Local database configuration

Set `ConnectionStrings:DefaultConnection` in local configuration or through an environment variable. Do not commit a real database password.

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=MealPrep;User ID=root;Password=your-password;"
```

After the EF CLI is installed, create and apply the initial schema from this directory:

```powershell
dotnet ef migrations add InitialIdentity
dotnet ef database update
```
