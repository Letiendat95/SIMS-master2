# 🚀 How to Run the SIMS Project

## **Quick Start (2 Minutes)**

### Option 1: Using Visual Studio (Recommended)

1. **Open the project**
   - Open Visual Studio 2026
   - Click `File` → `Open` → `Project/Solution`
   - Navigate to: `C:\Users\User\Downloads\SIMS.Web (1)\SIMS.sln`
   - Click `Open`

2. **Wait for dependencies** (first time only)
   - Visual Studio will automatically restore NuGet packages
   - This may take 1-2 minutes

3. **Start the app**
   - Press **F5** (Debug) or **Ctrl+F5** (Release)
   - **OR** Click the green play button ▶️ in the toolbar
   - **OR** Go to `Debug` → `Start Debugging`

4. **Login page appears**
   - Browser opens automatically to: `https://localhost:7107`
   - Use default credentials (see [Login Credentials](#login-credentials))

---

## **Prerequisites Check**

Before running, ensure you have:

| Requirement | Status | Action |
|------------|--------|--------|
| .NET 8 SDK | ✅ Required | Install from dotnet.microsoft.com |
| Visual Studio 2026 | ✅ Required | Already installed |
| SQL Server LocalDB | ✅ Required | Usually comes with VS |
| Connection to database | ⚠️ Check | See troubleshooting below |

### Check .NET 8 Installation

Open PowerShell and run:
```powershell
dotnet --version
```

Should show `8.0.x` or higher.

### Check SQL Server LocalDB

Open PowerShell and run:
```powershell
sqllocaldb info
```

Should list available LocalDB instances.

---

## **Step-by-Step Guide**

### Step 1: Open Visual Studio

1. Launch Visual Studio 2026 from Start menu
2. Click `File` → `Open` → `Project/Solution`
3. Browse to: `C:\Users\User\Downloads\SIMS.Web (1)\SIMS.sln`
4. Click `Open`

### Step 2: Wait for project load

- Visual Studio loads the solution
- NuGet packages restore automatically
- Wait for the status bar to show "Ready"

**Timeline:**
- First run: 2-3 minutes (packages restore)
- Subsequent runs: 30 seconds

### Step 3: Verify appsettings.json

Check that the database connection string is valid:

1. In Solution Explorer, find `appsettings.json`
2. Open it and verify the connection string:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=DESKTOP-F9BHFBG;Database=sims1;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Note**: Replace `DESKTOP-F9BHFBG` with your computer name if different.

To find your computer name:
```powershell
# In PowerShell
$env:COMPUTERNAME
```

### Step 4: Build the project

1. Press **Ctrl+Shift+B** (Build)
2. Wait for build to complete
3. Check Output window: "Build successful"

### Step 5: Run the project

1. Press **F5** (Start with Debugging)
2. Visual Studio starts the application
3. Browser opens to: `https://localhost:7107`
4. Login page appears

---

## **Login Credentials**

After running, the database is automatically seeded with these users:

### Admin Account
```
Username: admin@sims.edu
Password: admin123
Role: Administrator (full system access)
```

### Faculty Account
```
Username: faculty1@sims.edu
Password: faculty123
Role: Faculty (grade input, course view)
```

### Student Account
```
Username: student1@sims.edu
Password: student123
Role: Student (enrollment, record view)
```

---

## **Alternative Run Methods**

### Option 2: Using Command Line

Open PowerShell in the project directory and run:

```powershell
# Navigate to project
cd "C:\Users\User\Downloads\SIMS.Web (1)"

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the project
dotnet run --project SIMS.Web/SIMS.Web.csproj
```

Then open browser to: `https://localhost:7107`

### Option 3: Using IIS Express

1. In Visual Studio, click the dropdown next to the play button
2. Select `IIS Express` or the project name
3. Press F5 to run

---

## **First Run Experience**

### What happens:

1. ✅ Program.cs runs `DbInitializer.Initialize(context)`
2. ✅ Database is created (if not exists)
3. ✅ Sample data is seeded:
   - 3 Roles (Admin, Faculty, Student)
   - 6 Users (1 Admin, 2 Faculty, 3 Students)
   - 3 Courses
   - 4 Enrollments
   - Academic Records

4. ✅ App loads at `https://localhost:7107`
5. ✅ Login page displays
6. ✅ You can login with sample credentials

### Expected output in Console:

```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7107
info: Microsoft.Hosting.Lifetime[0]
	  Application started. Press Ctrl+C to exit
```

---

## **Database Connection**

### Using LocalDB

The project uses SQL Server LocalDB by default:

```
Server: (localdb)\mssqllocaldb
Database: sims1
Authentication: Windows (Trusted Connection)
```

**To access the database manually:**

```powershell
# Connect via command line
sqlcmd -S "(localdb)\mssqllocaldb" -d sims1

# Or use SQL Server Management Studio (SSMS)
# Server: (localdb)\mssqllocaldb
# Database: sims1
```

### Using Full SQL Server

If you have SQL Server installed, modify `appsettings.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=sims1;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## **Troubleshooting**

### Problem: "Connection string is invalid"

**Solution:**
1. Check your computer name matches in `appsettings.json`
2. Verify LocalDB is running:
   ```powershell
   sqllocaldb info
   ```
3. If not present, create an instance:
   ```powershell
   sqllocaldb create mssqllocaldb
   sqllocaldb start mssqllocaldb
   ```

### Problem: "Build failed with errors"

**Solution:**
1. Clean solution: `Build` → `Clean Solution`
2. Restore NuGet: `Tools` → `NuGet Package Manager` → `Package Manager Console`
3. Run: `Update-Package -Reinstall`
4. Rebuild: `Ctrl+Shift+B`

### Problem: "Database already exists error"

**Solution:**
The database might be locked. Either:
1. Delete the database and restart (auto-recreates)
   - Launch `Sql Server Management Studio`
   - Right-click database `sims1` → Delete
2. Or stop any running instances:
   ```powershell
   sqllocaldb stop mssqllocaldb
   sqllocaldb start mssqllocaldb
   ```

### Problem: "HTTPS certificate not trusted"

**Solution:**
This is normal for localhost. Click "Advanced" → "Proceed anyway" or:
1. Install dev certificate:
   ```powershell
   dotnet dev-certs https --trust
   ```
2. Restart the application

### Problem: "Cannot access https://localhost:7107"

**Solution:**
1. Check the console output for the actual port (might be 7108, 7109, etc.)
2. Check if another app is using port 7107
3. Change the port in `launchSettings.json`:
   ```json
   "applicationUrl": "https://localhost:5001;http://localhost:5000"
   ```

### Problem: "NuGet package restore fails"

**Solution:**
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# In Package Manager Console
Clear-NuGetCache

# Then restore
dotnet restore SIMS.sln
```

---

## **Performance Tips**

### Make it start faster:

1. **Run without debugging** (Ctrl+F5)
   - Faster than F5
   - You can still view browser console

2. **Use Release configuration**
   - Instead of Debug
   - Faster startup and execution
   - Select from dropdown in toolbar

3. **Disable HTTPS for local testing**
   - In `launchSettings.json`
   - Remove HTTPS redirection (not recommended for production)

### Optimize database access:

1. **LocalDB vs Full SQL Server**
   - LocalDB: Faster for development
   - SQL Server: Better for testing multi-user scenarios

---

## **Project Structure**

Once running, the application structure is:

```
https://localhost:7107/
├── / (Home redirect to Login)
├── /Account/Login (Login page)
├── /Student/Index (Student list)
├── /Student/Detail/{id} (Student profile)
├── /Course/Index (Course list)
├── /Course/Detail/{id} (Course details)
├── /Admin/Index (Admin dashboard)
├── /Faculty/InputGrade (Grade input)
└── /Account/Logout (Logout)
```

---

## **Navigation by Role**

After login, you see different menus based on your role:

### 👨‍💼 Admin
- Dashboard
- Student Management
- Course Management
- Role Management
- Register Student

### 👨‍🏫 Faculty
- My Courses
- Input Grades
- View Students

### 👨‍🎓 Student
- My Courses
- My Grades
- My Profile

---

## **Development Workflow**

### Making code changes:

1. Edit files in Visual Studio
2. Save (Ctrl+S)
3. If no immediate refresh:
   - Stop app (Shift+F5)
   - Press F5 to restart
   - Or use Hot Reload (Visual Studio 2022+)

### Testing changes:

1. Make code change
2. Rebuild (Ctrl+Shift+B)
3. Run (F5)
4. Test in browser
5. Check console for errors

### Debugging:

1. Set breakpoint: Click on line number
2. Press F5 to run with debugging
3. Step through code with F10/F11
4. View variables in Debug windows

---

## **Stopping the Application**

### Methods to stop:

1. **In Visual Studio**
   - Press Shift+F5
   - Click the stop button (⏹️)
   - Close the browser tab

2. **In Command Line**
   - Press Ctrl+C

3. **In Browser**
   - Close the tab (app keeps running in VS)
   - Press Shift+F5 to fully stop

---

## **Environment Variables**

The application uses the default ASP.NET Core environment:

- **Development**: Full error details, no caching
- **Production**: Error logging, optimizations enabled

Check current environment in `launchSettings.json`:

```json
{
  "profiles": {
	"SIMS.Web": {
	  "commandName": "Project",
	  "environmentVariables": {
		"ASPNETCORE_ENVIRONMENT": "Development"
	  }
	}
  }
}
```

---

## **Helpful Shortcuts**

| Action | Shortcut |
|--------|----------|
| Run App | F5 |
| Run without Debug | Ctrl+F5 |
| Stop App | Shift+F5 |
| Build Project | Ctrl+Shift+B |
| Clean Solution | Build → Clean Solution |
| Rebuild Solution | Build → Rebuild Solution |
| Open Package Manager | Tools → NuGet Package Manager |
| Open Terminal | Ctrl+` |

---

## **Logs & Debugging**

### View console output:

1. During debug (F5), console appears at bottom
2. Check "Output" window
3. Filter by "Debug" category
4. Look for startup messages

### Database queries:

Entity Framework logs queries to console in Development mode:

```
Microsoft.EntityFrameworkCore.Database.Command: Information
SELECT [u].[UserId], [u].[Username], [u].[PasswordHash], ...
```

---

## **Next Steps**

After running successfully:

1. ✅ **Login** with provided credentials
2. ✅ **Explore** the interface
3. ✅ **Create** new students/courses
4. ✅ **Test** different roles
5. ✅ **Review** the code in Visual Studio
6. ✅ **Make changes** and see real-time effects

---

## **Common Questions**

**Q: How do I reset the database?**  
A: Delete `sims1` database in SQL Server Management Studio; it auto-recreates on next run.

**Q: How do I change the port?**  
A: Edit `launchSettings.json` and change `applicationUrl`.

**Q: How do I use Visual Studio Code instead?**  
A: Open folder, run `dotnet run`, but most features require Visual Studio.

**Q: How do I deploy to production?**  
A: See `DEPLOYMENT_GUIDE.md` for Azure, IIS, and Docker options.

**Q: Can I run headless (without browser)?**  
A: Yes, use `dotnet run` in command line; access via browser separately.

---

## **Quick Checklist**

Before running:
- ✅ .NET 8 SDK installed
- ✅ Visual Studio 2026 open
- ✅ `SIMS.sln` opened
- ✅ `appsettings.json` has valid connection string
- ✅ SQL Server LocalDB available

To run:
- ✅ Press F5 or click play button
- ✅ Wait for browser to open
- ✅ Login with credentials
- ✅ Explore the app

---

**You're ready to run SIMS! 🎉**

Press **F5** now and enjoy! 🚀

---

**Last Updated**: 2024  
**Status**: Complete  
**Version**: 1.0
