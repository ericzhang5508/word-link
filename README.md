# Word-Link

A daily crossword application built with ASP.NET Core 10.0 MVC, SignalR, and SQLite. It features a minimalist Japanese "Silent Authority" design and real-time global solve counts.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Debugging & Local Development](#debugging--local-development)
- [Testing](#testing)
- [Deployment](#deployment)

---

## Prerequisites

Ensure you have the following installed on your local machine:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- IDE of your choice (Visual Studio 2022, Jetbrains Rider, or Visual Studio Code with the C# extension)
- Tailwind CSS CLI (if you are re-building styles on the frontend)
- [dotnet ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) CLI tool for managing the SQLite database.

---

## Debugging & Local Development

1. **Clone or Open the Project:**
   Open the root directory (`{your local path here}/word-link`) in your IDE.

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Database Setup (SQLite):**
   The project uses Entity Framework Core with an SQLite database. Before running the app for the first time, apply any pending migrations to create the local `app.db` file.
   ```bash
   dotnet ef database update
   ```

4. **Running the Application:**
   Run the project using your IDE's debug and run features (e.g., F5 in VS Code or Visual Studio) to enable breakpoints and hot reload.
   Alternatively, you can run the app from the terminal via the .NET CLI:
   ```bash
   dotnet watch run
   ```
   *(Using `dotnet watch run` enables hot reload for C# file and Razor `.cshtml` file changes).*

5. **Tailwind CSS:**
   If you make changes to the Tailwind CSS classes within your Razor pages, you will need to re-compile the CSS. Usually, this is handled via a watch script or running the Tailwind CLI:
   ```bash
   # Make sure you are in the directory containing your package.json / tailwind config
   npx tailwindcss -i ./wwwroot/css/site.css -o ./wwwroot/css/output.css --watch
   ```
   *(Ensure paths match your Tailwind setup).*

---

## Testing

To run any associated tests within the solution:

1. **Run All Tests:**
   If you have a separate testing project (e.g., `WordLink.Tests`), navigate to the root or test project directory and run:
   ```bash
   dotnet test
   ```

2. **Debugging Tests:**
   You can debug tests directly within Visual Studio or VS Code by using the Test Explorer panel and selecting "Debug Test".

*(Note: If a dedicated test project has not been created yet, you can create one using `dotnet new xunit -n WordLink.Tests` and add a reference to the main project: `dotnet run reference WordLink.Tests WordLink.csproj`.)*

---

## Deployment

To prepare the application for a production environment (such as a Windows Server/IIS, Linux VPS, or an Azure App Service):

1. **Publish the App:**
   Create a release build of the application containing all compiled code, Views, and static assets.
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Database Considerations:**
   - **SQLite File Path:** Ensure that your production `appsettings.json` has a connection string pointing to a valid, persistent path on your host server. The application pool or executing user **must** have read/write access to both the SQLite `.db` file and the directory it resides in (since SQLite needs to create temporary journal files).
   - **Migrations:** Before going live, make sure to apply the latest EF Core migrations to the production environment database. For production, generating a SQL script from migrations is generally preferred:
     ```bash
     dotnet ef migrations script
     ```

3. **Hosting:**
   - **Windows (IIS):** Install the [.NET Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0). Point an IIS site to the `publish` folder and set the Application Pool to "No Managed Code".
   - **Linux (Nginx/Apache):** Set up a reverse proxy using Nginx or Apache configured to forward requests to the Kestrel server running on a local port (e.g., 5000) using a `systemd` service to keep the app running.
   - **Docker:** You can alternatively containerize the application using a `Dockerfile` with the `mcr.microsoft.com/dotnet/aspnet:10.0` runtime image.

4. **SignalR Configuration:**
   If deploying to a load-balanced environment (multiple servers or instances), ensure you configure a Redis backplane for SignalR so real-time crossword solve counts can be broadcast across all instances. (If running on a single instance, the default in-memory SignalR setup is sufficient).

## License

This project is licensed under the terms of the [GPL-3.0](https://www.gnu.org/licenses/gpl-3.0.html) license.
