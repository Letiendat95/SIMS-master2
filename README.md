# Student Information Management System (SIMS)

## Overview
The Student Information Management System (SIMS) is a modern, scalable web application built with .NET 8 and ASP.NET Core MVC to manage student information, courses, enrollments, and academic records for universities.

## Features

### 1. **Student Registration & Management**
- Efficient registration of new students
- Capture and store essential student information (personal details, contact info, academic program)
- View student profiles with comprehensive details
- Edit student information
- Track academic records and enrollments

### 2. **Course Management**
- Create and manage courses offered by the university
- Assign faculty members to courses
- Set course capacity and track enrollment
- View course details and enrolled students
- Track course status (Active/Inactive)

### 3. **User Authentication & Authorization**
- Secure login system with password hashing (BCrypt)
- Session-based authentication
- Role-based access control (RBAC) with three roles:
  - **Admin**: Full system access, manage students, courses, and users
  - **Faculty**: Can input grades, view courses and enrollments
  - **Student**: Can view courses, check academic records

### 4. **Enrollment Management**
- Assign students to courses
- Track enrollment status
- Input grades for enrolled students
- View course enrollment history

### 5. **Academic Records**
- Track student GPA and credits completed
- View enrollment history with grades
- Academic record management

## Technical Architecture

### Technology Stack
- **Framework**: .NET 8
- **Web Platform**: ASP.NET Core MVC
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Security**: BCrypt.Net-Next for password hashing
- **UI Framework**: Bootstrap 5
- **Icons**: Bootstrap Icons

### Project Structure
```
SIMS.Web/
├── Models/               # Domain models (User, Student, Course, Enrollment, etc.)
├── Controllers/          # MVC controllers (Account, Student, Course, Admin, Faculty)
├── Services/             # Business logic layer
├── Repositories/         # Data access layer
├── Data/                 # Entity Framework DbContext and initializer
├── Helpers/              # Authorization attributes and utilities
├── ViewModels/           # View-specific models
├── Views/                # Razor templates
├── wwwroot/              # Static assets
├── Program.cs            # Application startup configuration
├── appsettings.json      # Configuration settings
└── SIMS.Web.csproj       # Project file

```

### Database Schema
The system uses Table Per Hierarchy (TPH) inheritance for user types:
- **Users** (base table)
  - **Students** (inherits from User)
  - **Faculty** (inherits from User)
  - **Administrators** (inherits from User)
- **Courses**
- **Enrollments** (join table between Students and Courses)
- **AcademicRecords**
- **Roles**

### Design Patterns Used
1. **Repository Pattern**: Data access abstraction with `IRepository<T>` interface
2. **Service Layer Pattern**: Business logic separation with service classes
3. **Dependency Injection**: Built-in ASP.NET Core DI container
4. **Custom Authorization Attribute**: Role-based access control

## Installation & Setup

### Prerequisites
- .NET 8 SDK or later
- SQL Server (Express or higher)
- Visual Studio 2022 or VS Code with C# extensions

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/SVHuyn/SIMS.git
   cd SIMS.Web
   ```

2. **Configure the database connection**

   **Option A: Remote SQL Server (Recommended - Miễn phí)**
   
   Kết nối đến database chung trên máy chủ:
   ```bash
   cd SIMS.Web
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:192.168.99.44,1433;Database=sims1;User ID=remote_user;Password=Remote@123456;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
   ```
   
   **Lưu ý:** Phải cùng mạng LAN hoặc port 1433 phải mở trên router

   **Option B: Azure SQL Database**
   
   Use User Secrets to store your connection string securely:
   ```bash
   cd SIMS.Web
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=sims1;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
   ```
   
   Replace:
   - `YOUR_SERVER`: Your Azure SQL server name
   - `YOUR_USERNAME`: Database username
   - `YOUR_PASSWORD`: Database password

   **Option C: Local SQL Server**
   
   Update `appsettings.json`:
   ```json
   {
	 "ConnectionStrings": {
	   "DefaultConnection": "Server=YOUR_SERVER;Database=sims1;Trusted_Connection=True;TrustServerCertificate=True;"
	 }
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

   The application will start at `https://localhost:5001`

## Default Credentials

The system comes with pre-populated sample data for testing:

### Admin Account
- **Username**: admin
- **Password**: admin123
- **Email**: admin@sims.edu

### Faculty Accounts
- **Username**: dr.smith
- **Password**: faculty123
- **Email**: dr.smith@sims.edu
- **Department**: Computer Science

- **Username**: dr.johnson
- **Password**: faculty123
- **Email**: dr.johnson@sims.edu
- **Department**: Mathematics

### Student Accounts
- **Username**: john.doe
- **Password**: student123
- **Email**: john.doe@student.sims.edu
- **Program**: Computer Science

- **Username**: jane.smith
- **Password**: student123
- **Email**: jane.smith@student.sims.edu
- **Program**: Computer Science

- **Username**: michael.brown
- **Password**: student123
- **Email**: michael.brown@student.sims.edu
- **Program**: Mathematics

## Key Releases & Version Information

### Version 1.0 - Initial Release
- Complete user authentication system with BCrypt password hashing
- Student registration and management
- Course creation and management
- Enrollment system with grade input
- Academic record tracking
- Role-based access control (Admin, Faculty, Student)
- Responsive Bootstrap 5 UI
- Session-based state management

## Non-Functional Requirements Implementation

### Scalability ✅
- Stateless application architecture
- Scalable data access layer with repository pattern
- Support for multiple database profiles
- Async/await patterns for I/O operations

### Performance ✅
- Entity Framework Core with eager loading optimization
- Indexed database queries
- Efficient session management
- Minimal view rendering overhead

### Security ✅
- BCrypt password hashing with salting
- Role-based authorization attributes
- Session-based authentication
- HTTPS enforcement
- SQL Server parameterized queries (via EF Core)

### Usability ✅
- Intuitive Bootstrap 5 interface
- Clear navigation structure
- Contextual role-based menu items
- User-friendly forms with validation messages
- Responsive design for mobile devices

### Accessibility ✅
- Semantic HTML markup
- Bootstrap accessibility features
- ARIA labels and roles
- Keyboard navigation support
- Color-independent information display

### Reliability ✅
- Comprehensive error handling
- Transaction support for multi-step operations
- Database integrity constraints
- Session state management
- Graceful error pages

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER;Database=sims;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  },
  "AllowedHosts": "*"
}
```

## SOLID Principles Implementation

1. **Single Responsibility**: Each service/controller has one reason to change
2. **Open/Closed**: Repository pattern allows extension without modification
3. **Liskov Substitution**: `IRepository<T>` can be substituted with any implementation
4. **Interface Segregation**: Focused interfaces (e.g., `IRepository<T>`)
5. **Dependency Inversion**: Depends on abstractions, not concrete implementations

## Future Enhancements

- [ ] Email notifications (enrollment confirmations, grade updates)
- [ ] Advanced reporting and analytics dashboard
- [ ] GPA calculation automation
- [ ] Course prerequisites system
- [ ] Student attendance tracking
- [ ] Mobile app integration (React Native/Flutter)
- [ ] Multi-language support
- [ ] Advanced search and filtering
- [ ] Document upload (transcripts, certificates)
- [ ] Payment integration for student fees
- [ ] API layer (RESTful API for external integration)

## Troubleshooting

### Connection String Issues
Ensure SQL Server is running and the connection string correctly specifies your server name and database.

### Database Migration Errors
Clear the database and restart the application to trigger the initializer.

### Session Issues
Verify that `AddSession()` and `UseSession()` are configured in Program.cs.

## Contributing
Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Commit your changes with clear messages
4. Push to the branch
5. Create a Pull Request

## License
This project is licensed under the MIT License.

## Support
For issues or questions, please open an issue on the GitHub repository.

---

**Built with ❤️ for educational purposes**

Last Updated: 2024
