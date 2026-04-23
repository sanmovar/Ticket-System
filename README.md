# Ticket System

A web-based ticket management system built with ASP.NET Core MVC for structured internal support and project workflows.

## Overview

This project is a practical ticket management application that allows users to create, manage and track tickets in a clear and structured way.  
It was built to strengthen backend, database and full-stack web development skills with a focus on clean architecture, usability and real-world workflows.

## Features

- User authentication and authorization
- Role-based access control
- Create, edit and manage tickets
- Manage projects and related ticket assignments
- Admin area for extended management functions
- Database integration with Entity Framework Core
- Structured user workflows with a clean interface

## Tech Stack

### Backend
- C#
- ASP.NET Core MVC
- Entity Framework Core

### Frontend
- HTML
- CSS
- JavaScript

### Database
- SQL Server
- PostgreSQL

### Tools
- Git
- Visual Studio

## Screenshots

Add screenshots here to show the UI and important features.

### Suggested screenshots
- Dashboard / Home page
- Ticket overview
- Ticket details page
- Admin panel
- Project management page

Example:

## Screenshots

### Home / Dashboard
<img src="./screenshots/dashboard.png" alt="Dashboard" width="900" />

### Ticket Overview
<img src="./screenshots/tickets-overview.png" alt="Ticket overview" width="900" />

### Ticket Details
<img src="./screenshots/ticket-details.png" alt="Ticket details" width="900" />

### Admin Panel
<img src="./screenshots/admin-panel.png" alt="Admin Panel" width="900" />

## Purpose

The goal of this project is to build a realistic web application that demonstrates:
- backend development with ASP.NET Core MVC
- database handling with Entity Framework Core
- role-based user management
- structured CRUD operations
- practical full-stack development skills

## Getting Started

### Requirements

- .NET SDK
- Visual Studio or Visual Studio Code
- SQL Server or PostgreSQL
- Git

### Installation

1. Clone the repository:

```bash
git clone https://github.com/sanmovar/Ticket-System.git
```

2. Open the project in Visual Studio.

3. Restore NuGet packages.

4. Configure the database connection in `appsettings.json`.

5. Apply migrations:

```bash
dotnet ef database update
```

6. Run the project:

```bash
dotnet run
```

## Project Structure

```text
Ticket-System/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── wwwroot/
├── appsettings.json
└── Program.cs
```

## Learning Focus

This project was created to improve practical knowledge in:
- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL and database design
- authentication and authorization
- clean UI structure in web applications

## Future Improvements

- Better filtering and search for tickets
- Improved dashboard statistics
- File upload support for tickets
- Notifications for status changes
- More responsive UI improvements
- Deployment-ready production setup

## Author

**Andrey Movshovich**  
Junior .NET / Web Developer in retraining

## License

This project is for learning and portfolio purposes.
