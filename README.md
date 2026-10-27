# BlogSphere API

A RESTful Blog Application API built using ASP.NET Core Web API and Entity Framework Core.

## About the Project

BlogSphere API is a backend application for managing users, blog posts, and comments. It provides RESTful endpoints and uses JWT authentication to protect selected operations.

## Technologies Used

- C#
- ASP.NET Core Web API
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

- .NET 8 SDK
- Visual Studio 2022
- SQL Server / SQL Server Express
- Git

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/sakshiverma-dev19/BlogSphere.API.git
cd BlogSphere.API
```

Open `BlogSphere.API.sln` in Visual Studio 2022.

### 2. Configure the application

Configure your local database connection string under `ConnectionStrings:DefaultConnection`.

Configure the following JWT settings:

- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryMinutes`

Use a strong, private JWT key. Do not commit real secrets or database credentials to GitHub.

### 3. Set up the database

The repository includes `Database/BlogSphereDb.sql` as a database setup script.

Run the SQL script in SQL Server Management Studio (SSMS) if it matches the current database schema. Alternatively, use Entity Framework Core migrations if the project migrations are up to date.

Do not apply migrations and the SQL script blindly to the same database. Confirm the intended setup method first.

### 4. Run the API

Run the `BlogSphere.API` project from Visual Studio.

In the Development environment, open Swagger UI at:

`https://localhost:{port}/swagger`

Replace `{port}` with the HTTPS port configured for your application.

## Authentication

Register or log in using the authentication endpoints. The login endpoint returns a JWT token.

For protected endpoints, include the token in the request header:

```text
Authorization: Bearer YOUR_JWT_TOKEN
```

In Swagger UI, select **Authorize** and enter your token as instructed by the Swagger interface.

## Running Tests

Run the following command from the solution folder:

```bash
dotnet test
```

## Author

Sakshi Verma

GitHub: [https://github.com/sakshiverma-dev19](https://github.com/sakshiverma-dev19)
