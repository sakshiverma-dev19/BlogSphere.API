# BlogSphere API

A RESTful Blog Application API built using ASP.NET Core Web API and Entity Framework Core.

## About the Project

BlogSphere API is a backend application for managing users, blog posts, and comments. It provides RESTful endpoints and uses JWT authentication to protect selected operations.

## Technologies Used

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI
- xUnit
- ASP.NET Core Integration Testing

## Features

- User registration and login
- Password hashing
- JWT-based authentication and authorization
- Create and retrieve blog posts
- Update and delete posts by their authors
- Create, retrieve, update, and delete comments
- Authorization for protected endpoints
- Swagger API documentation
- Automated integration tests
- SQL database setup script

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Auth/register` | Register a new user |
| POST | `/api/Auth/login` | Log in and receive a JWT token |

### Posts

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Posts` | Retrieve all posts |
| GET | `/api/Posts/{id}` | Retrieve a post by ID |
| POST | `/api/Posts` | Create a post (authentication required) |
| PUT | `/api/Posts/{id}` | Update your own post (authentication required) |
| DELETE | `/api/Posts/{id}` | Delete your own post (authentication required) |

### Comments

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Comments` | Retrieve all comments |
| GET | `/api/Comments?post_id={postId}` | Retrieve comments for a post |
| GET | `/api/Comments/{id}` | Retrieve a comment by ID |
| POST | `/api/Comments` | Create a comment (authentication required) |
| PUT | `/api/Comments/{id}` | Update your own comment (authentication required) |
| DELETE | `/api/Comments/{id}` | Delete your own comment (authentication required) |

## Project Structure

```text
BlogSphere.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── PostsController.cs
│   └── CommentsController.cs
├── Data/
│   └── BlogSphereDbContext.cs
├── Database/
│   └── BlogSphereDb.sql
├── Migrations/
├── Models/
├── Program.cs
└── appsettings.json

BlogSphere.API.Tests/
├── BlogSphereIntegrationTests.cs
└── UnitTest1.cs
```

## Prerequisites

Before running this project, install:

- .NET 8 SDK
- Visual Studio 2022
- SQL Server or SQL Server Express
- SQL Server Management Studio (SSMS)
- Git

## Getting Started

### 1. Clone the Repository

Open a terminal and run:

```bash
git clone https://github.com/sakshiverma-dev19/BlogSphere.API.git
cd BlogSphere.API
```

Open `BlogSphere.API.sln` in Visual Studio 2022.

### 2. Configure the Database Connection

Open:

`BlogSphere.API/appsettings.json`

Find the `DefaultConnection` setting and replace `YOUR_SQL_SERVER_NAME` with your local SQL Server instance name.

Example connection string for SQL Server Express:

```text
Server=.\SQLEXPRESS;Database=BlogSphereDb;Trusted_Connection=True;TrustServerCertificate=True;
```

For another SQL Server instance, use its actual server name.

### 3. Set Up the Database

The repository includes the SQL setup script:

`BlogSphere.API/Database/BlogSphereDb.sql`

To create the database:

1. Open SQL Server Management Studio (SSMS).
2. Connect to your SQL Server instance.
3. Open `BlogSphereDb.sql`.
4. Execute the script if it matches the current database schema.

Alternatively, use Entity Framework Core migrations if they are up to date.

Do not run both the SQL script and migrations blindly against the same database. Choose the intended setup method.

### 4. Configure JWT Authentication

The application requires these JWT settings:

- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryMinutes`

The Issuer, Audience, and ExpiryMinutes settings are available in `appsettings.json`.

For local development, configure the JWT key using .NET User Secrets.

From the `BlogSphere.API` project directory, run:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_PRIVATE_LONG_RANDOM_SECRET_KEY"
```

Use your own strong secret key. Never commit real JWT keys, passwords, or private credentials to GitHub.

### 5. Restore Dependencies

Open a terminal in the solution folder and run:

```bash
dotnet restore
```

### 6. Run the API

In Visual Studio:

1. Set `BlogSphere.API` as the startup project.
2. Select the HTTPS launch profile.
3. Click Run.

Swagger UI will be available at:

`https://localhost:{port}/swagger`

Replace `{port}` with the HTTPS port configured for your application.

### 7. Authentication

Register or log in using the authentication endpoints.

The login endpoint returns a JWT token.

For protected endpoints, include the token in the request header:

```text
Authorization: Bearer YOUR_JWT_TOKEN
```

In Swagger UI, click **Authorize** and enter your token as instructed by the Swagger interface.

## Running Tests

Open a terminal in the solution folder and run:

```bash
dotnet test
```

This command builds the solution and runs the automated tests.

All tests should pass before deployment.

## Author

**Sakshi Verma**

GitHub: [sakshiverma-dev19](https://github.com/sakshiverma-dev19)
