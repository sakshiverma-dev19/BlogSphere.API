\# BlogSphere API



A RESTful Blog Application API built using ASP.NET Core Web API and Entity Framework Core.



\## About the Project



BlogSphere API is a backend application designed to manage blog posts, user authentication, and comments. It provides API endpoints for working with blog content and protects selected endpoints using JWT authentication.



\## Technologies Used



\- C#

\- ASP.NET Core Web API

\- Entity Framework Core

\- SQL Server

\- JWT Authentication

\- RESTful APIs

\- xUnit

\- ASP.NET Core Integration Testing



\## Features



\- User registration and login

\- JWT-based authentication

\- Create and retrieve blog posts

\- Manage comments

\- Authorization for protected endpoints

\- Integration tests for API endpoints

\- Entity Framework Core database migrations



\## Project Structure



```text

BlogSphere.API/

├── Controllers/

│   ├── AuthController.cs

│   ├── PostsController.cs

│   └── CommentsController.cs

├── Data/

│   └── BlogSphereDbContext.cs

├── Migrations/

├── Models/

├── Program.cs

└── appsettings.json



BlogSphere.API.Tests/

├── BlogSphereIntegrationTests.cs

└── UnitTest1.cs

```



\## Getting Started



\### Prerequisites



\- .NET SDK compatible with the project

\- Visual Studio 2022

\- SQL Server

\- Git



\### Setup



1\. Clone the repository:



&#x20;  ```bash

&#x20;  git clone https://github.com/sakshiverma-dev19/BlogSphere.API.git

&#x20;  ```



2\. Open `BlogSphere.API.sln` in Visual Studio.



3\. Configure the database connection string in your local configuration.



4\. Apply Entity Framework Core migrations to create the database.



5\. Run the `BlogSphere.API` project.



\## Running Tests



Run the following command from the solution folder:



```bash

dotnet test

```



\## Authentication



Register or log in through the authentication endpoints to obtain a JWT token. Include the token in the authorization header when calling protected endpoints:



```text

Authorization: Bearer YOUR\_JWT\_TOKEN

```



\## Future Improvements



\- Add pagination and filtering for blog posts

\- Improve API validation and error handling

\- Expand unit and integration test coverage

\- Add API documentation and example requests



\## Author



Sakshi Verma



GitHub: \[sakshiverma-dev19](https://github.com/sakshiverma-dev19)

