# Authentication API

This is an **ASP.NET Core REST API** project for user authentication. It is implemented using **Clean Architecture** and is **Dockerized**.

## Features

- User registration and login
- Authentication with JWT Token
- User management
- Uses **Entity Framework Core** and **Code First Migrations**
- **Docker Compose** for easy deployment

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/)
- [Git](https://git-scm.com/)
- **SQL Server** database

## Installation and Running the Project

### 1. Clone the Repository

```sh
git clone https://github.com/username/authentication-api.git
cd authentication-api
```

### 2. Run the Project with Docker

```sh
docker-compose up --build
```

### 3. Run the Project Without Docker

#### 3.1. Apply Migrations

```sh
dotnet ef database update
```

#### 3.2. Run the Project

```sh
dotnet run --project AuthenticationApp
```

## Project Structure

```
.
├── Application
│   ├── Services         # Business Logic Layer
│   ├── DTOs            # Data Transfer Objects
│   ├── Interfaces       # Interfaces for Services and Repositories
│   ├── Mapper          # Mapper Configurations
│
├── AuthenticationApp
│   ├── Controllers      # API Controllers
│   ├── Program.cs      # Application Entry Point
│   ├── appsettings.json # Project Settings
│   ├── Dockerfile      # Docker Configuration
│
├── Domain
│   ├── Entities        # Database Models
│   ├── Enum            # Constant Values (e.g., User Status)
│
├── Infrastructure
│   ├── DbContext       # Database Configuration
│   ├── Repositories    # Repository Implementations
│   ├── Migrations      # Migration Files
│
├── docker-compose.yml  # Docker Compose File
└── README.md           # This File :)
```

## Configuration

You can configure the application using either **environment variables** or an **appsettings.json** file.

### Environment Variables

To configure environment variables, use the `.env` file or **Docker Compose** settings.

```env
LOGGING__LOGLEVEL__DEFAULT=Information
LOGGING__LOGLEVEL__MICROSOFT_ASPNETCORE=Warning

ALLOWED_HOSTS=*

CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=database;Database=AuthenticationApp;TrustServerCertificate=True;User Id=sa;Password=YourSecurePassword;MultipleActiveResultSets=True

JWT__AUDIENCE=https://your-audience-url
JWT__ISSUER=https://your-issuer-url
JWT__SIGNINKEY=your-secure-signing-key
```

### Using appsettings.json

Alternatively, you can create an `appsettings.json` file in the **AuthenticationApp** project and populate it with the following values:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=database;Database=AuthenticationApp;TrustServerCertificate=True;User Id=sa;Password=YourSecurePassword;MultipleActiveResultSets=True"
  },
  "JWT": {
    "Audience": "https://your-audience-url",
    "Issuer": "https://your-issuer-url",
    "SignInKey": "your-secure-signing-key"
  }
}
```

**Note:** Ensure that the database name in `docker-compose.yml` matches the `DefaultConnection` string in `appsettings.json`. Similarly, make sure the database password is consistent across both configurations.

## API Documentation

After running the project, API documentation is available via **Scalar** at:

```
http://localhost:{port}/scalar/v1
```

