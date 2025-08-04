# Authentication API

This is an **ASP.NET Core REST API** project for user authentication, implemented using **Clean Architecture**.

---

## ✨ Features

* ✅ User registration and login
* ✅ JWT-based Authentication
* ✅ Refresh Token implementation
* ✅ User management
* ✅ Entity Framework Core with Code First Migrations
* ✅ Serilog integration for logging (Console & Seq)

---

## 🔧 Prerequisites

* [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* [Git](https://git-scm.com/)
* **SQL Server**

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/username/authentication-api.git
cd authentication-api
```

### 2. Apply Migrations

```bash
dotnet ef database update
```

### 3. Run the Project

```bash
dotnet run --project AuthenticationApp
```

---

## 🧾 appsettings.json (Example)

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=database;Database=AuthenticationApp;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "JWT": {
    "Audience": "http://localhost:port",
    "Issuer": "http://localhost:port",
    "SignInKey": "key"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Seq" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "ServerUrl": "http://localhost:5341" } }
    ]
  }
}
```

---

## 🧱 Project Structure

```plaintext
.
├── Application
│   ├── DTOs               # Data Transfer Objects
│   ├── Interfaces         # Service and repository interfaces
│   ├── Mapper             # AutoMapper configurations
│   └── Services           # Business logic implementations
│
├── AuthenticationApp
│   ├── Controllers        # API controllers (e.g., AccountController)
│   ├── Middleware         # Custom middleware (e.g., exception handler)
│   ├── Program.cs         # Application entry point
│   └── appsettings.json   # App configuration
│
├── Domain
│   ├── Entities           # Domain models (User, Role, etc.)
│   ├── Enum               # Enums like UserStatus
│   └── Interfaces         # Domain-specific contracts or abstractions
│
├── Infrastructure
│   ├── DbContext          # EF Core database context
│   ├── Logging            # Logger adapters (e.g., Serilog wrapper)
│   └── Repositories       # Data access layer implementations
│
└── README.md              # This file

```

---

## 🔁 Refresh Token

A new endpoint has been added to the `AccountController` to handle **refreshing JWT tokens**.

### Endpoint

```
POST /api/account/refresh-token
```

### Request Body

```json
{
  "accessToken": "your-old-access-token",
  "refreshToken": "your-refresh-token"
}
```

### Response

```json
{
  "accessToken": "new-access-token",
  "refreshToken": "new-refresh-token"
}
```

### Notes:

* Tokens are securely stored and validated.
* Refresh token flow ensures continued access without forcing users to log in again.

---

## 📖 API Documentation

After running the project, visit:

```
http://localhost:{port}/scalar/v1
```

For interactive documentation using **Scalar**.

