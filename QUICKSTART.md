# SIMS Quick Start Guide

## 🚀 Getting Started in 5 Minutes

### Prerequisites
- .NET 8 SDK
- SQL Server (Express or above)
- Visual Studio 2022 / VS Code

---

## Step 1: Clone & Open
```bash
git clone https://github.com/SVHuyn/SIMS.git
cd SIMS.Web
```

Open `SIMS.sln` in Visual Studio 2022

---

## Step 2: Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=sims;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Replace `YOUR_SERVER_NAME`** with your SQL Server instance name (e.g., `DESKTOP-ABC123` or `.\SQLEXPRESS`)

---

## Step 3: Run the Application

In Visual Studio:
1. Press `F5` or click **Run**
2. Application will start at `https://localhost:5001`

The database will be created and seeded automatically.

---

## Step 4: Login with Test Credentials

### Option A: Admin Portal
- **URL**: `https://localhost:5001`
- **Username**: `admin`
- **Password**: `admin123`
- **Features**: Manage students, courses, view dashboard

### Option B: Faculty Portal
- **Username**: `dr.smith`
- **Password**: `faculty123`
- **Features**: Input grades, view courses

### Option C: Student Portal
- **Username**: `john.doe`
- **Password**: `student123`
- **Features**: View courses, check grades

---

## 📊 Main Features

### For Admin
- 📋 Student management dashboard
- ➕ Register new students
- 🔍 Search and filter students
- 📚 Manage courses
- 👥 Role management

### For Faculty
- 📖 View assigned courses
- 👨‍🎓 See enrolled students
- ✏️ Input and manage grades

### For Students
- 📚 Browse available courses
- 📊 View enrollments
- 📈 Check academic records and grades

---

## 🛠️ Project Structure

```
SIMS.Web/
├── Models/              # Data models
├── Controllers/         # Route handlers
├── Services/            # Business logic
├── Repositories/        # Data access
├── Data/                # Database config
├── Views/               # UI templates
├── Helpers/             # Authorization
└── appsettings.json     # Configuration
```

---

## 🔐 Security Features

- ✅ **BCrypt Password Hashing**: Secure password storage
- ✅ **Role-Based Authorization**: Admin, Faculty, Student roles
- ✅ **Session Management**: Secure session handling
- ✅ **HTTPS Enabled**: Encrypted communication

---

## 📱 Default Sample Data

### Roles
- Admin
- Faculty  
- Student

### Users
- **Admin**: admin@sims.edu
- **Faculty**: dr.smith@sims.edu, dr.johnson@sims.edu
- **Students**: 3 sample students

### Courses
- CS101: Introduction to Programming (3 credits)
- CS201: Advanced C# Programming (4 credits)
- MATH101: Calculus I (4 credits)

### Enrollments
Sample students are pre-enrolled in courses

---

## 🚨 Troubleshooting

### "Cannot connect to database"
- Check SQL Server is running
- Verify server name in `appsettings.json`
- Use SQL Server Management Studio to verify connection

### "Database already exists"
- Delete the database in SQL Server Management Studio
- Restart the application to recreate it

### "Port already in use"
- Change port in `launchSettings.json`
- Or stop application using the port

---

## 📚 Key Pages

| Page | URL | Access |
|------|-----|--------|
| Login | `/Account/Login` | Everyone |
| Student List | `/Student/Index` | Admin, Faculty, Students |
| Student Detail | `/Student/Detail/{id}` | Admin, Faculty, Students |
| Courses | `/Course/Index` | Admin, Faculty, Students |
| Admin Dashboard | `/Admin/Index` | Admin Only |
| Input Grade | `/Faculty/InputGrade` | Faculty, Admin |

---

## 💾 Database Schema

**Tables**: Users, Students, Faculties, Administrators, Courses, Enrollments, AcademicRecords, Roles

**Relationships**:
- Users → Roles (many-to-one)
- Students → Enrollments (one-to-many)
- Courses → Enrollments (one-to-many)
- Students → AcademicRecords (one-to-one)

---

## 🔄 Development Workflow

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature
   ```

2. **Make changes** to models, services, views

3. **Test** by running the application (F5)

4. **Commit and push**
   ```bash
   git add .
   git commit -m "Add: your feature description"
   git push origin feature/your-feature
   ```

5. **Create Pull Request** on GitHub

---

## 📈 Performance Considerations

- ✅ Async/await for all I/O operations
- ✅ Entity Framework Core with Include() for related data
- ✅ Session-based state (lightweight)
- ✅ SQL Server indexing on primary/foreign keys

---

## 🔧 Configuration Files

### `appsettings.json`
- Database connection string
- Logging configuration
- Allowed hosts

### `launchSettings.json`
- HTTPS binding
- Port configuration
- Environment settings

---

## 📖 Documentation

- **README.md**: Comprehensive project documentation
- **PROJECT_COMPLETION.md**: Implementation details and checklist
- **This file**: Quick start guide

---

## ✅ Quick Verification

After startup, verify:
- [ ] Can login with admin/admin123
- [ ] Admin dashboard loads
- [ ] Student list displays 3 sample students
- [ ] Can view course list (3 courses)
- [ ] Can see enrollments per course

---

## 🎓 Learning Resources

Topics covered in this project:
- ASP.NET Core MVC architecture
- Entity Framework Core with inheritance
- Repository and Service patterns
- Authentication and authorization
- Bootstrap responsive design
- SOLID principles
- Clean code practices

---

## 📞 Support

For issues:
1. Check the troubleshooting section above
2. Review `README.md` for detailed documentation
3. Check SQL Server connection
4. Verify .NET 8 SDK is installed: `dotnet --version`

---

## 🎉 You're Ready!

The SIMS application is now running. Explore the features and customize as needed!

**Happy coding! 🚀**
