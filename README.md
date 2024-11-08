iCareWebApplication Setup Guide


Prerequisites
----------------
.NET Core SDK (version compatible with the project, e.g., .NET 6.0 or .NET 5.0)
MySQL Server (version 5.7 or later)
MySQL Workbench (optional, for database management)


Steps to Set Up the Project
------------------------------
1. Unzip the Project
- Download the project zip file.
2. Unzip it to your preferred location on your computer.
3. Configure the Database Connection
- Before running the project, you'll need to update the database connection string to point to your MySQL instance.
- Open the project folder in your preferred code editor (e.g., Visual Studio Code or Visual Studio).
- Locate the appsettings.json file, typically found in the root of the project directory.
- Update the DefaultConnection in the ConnectionStrings section to use your MySQL database credentials. Replace localhost, DatabaseName, root, and YourPassword with your own MySQL server details.
  - Here is an example of what that would look like: 
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=DatabaseName;User=root;Password=YourPassword;"
    }"
Server: The MySQL server address (e.g., localhost if running locally).
Database: The name of the database you want to use for the project (e.g., iCareDatabase). Ensure the database exists in MySQL, or create it before proceeding.
User: Your MySQL username (e.g., root).
Password: Your MySQL password.
Save the changes.

3. Run Migrations to Set Up the Database Schema
- Entity Framework migrations are included in the project, allowing you to set up the necessary database tables.
- Open a terminal in the root of the project directory.
- Run the following command to apply the migrations and create the tables in the MySQL database:
  - dotnet ef database update
This command will apply all migrations, including the initial migration, creating the tables defined in the project.

4. Run the Project
With the database configured and the tables created, you're ready to run the project.

Run the project with the following command:
dotnet run
Open a browser and navigate to the local address where the application is running, typically http://localhost:5000 or as specified in the console output (mine was 7235)
- if this doesnt work you can also just run using the play button in Visual Studio

Troubleshooting
Error: Unable to connect to MySQL server
Double-check the connection string in appsettings.json to ensure it's correct.

Error: No migrations found
If you encounter an error indicating that no migrations were found, you may need to add the initial migration. Run:
- dotnet ef migrations add InitialCreate
- dotnet ef database update
Additional Notes
If you need sample data, you can manually add it using MySQL Workbench or any MySQL client, as this setup does not include seed data.
For any custom configurations, feel free to consult the .NET and Entity Framework documentation.
