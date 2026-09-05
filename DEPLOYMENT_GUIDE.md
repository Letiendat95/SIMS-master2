# SIMS Deployment Guide

## 🚀 Deployment Instructions

This guide covers deploying the Student Information Management System to various environments.

---

## 📋 Pre-Deployment Checklist

- [ ] Build successful (`dotnet build`)
- [ ] All tests pass
- [ ] Connection string configured
- [ ] SQL Server is accessible
- [ ] HTTPS certificate configured
- [ ] Environment variables set
- [ ] Database backups scheduled
- [ ] Logging configured

---

## 🏠 Local Development Setup

### Prerequisites
- .NET 8 SDK
- SQL Server Express (free)
- Visual Studio 2022 or VS Code

### Steps

1. **Clone Repository**
   ```bash
   git clone https://github.com/SVHuyn/SIMS.git
   cd SIMS.Web
   ```

2. **Configure Connection String**
   ```bash
   # Edit appsettings.json
   # Update: Server=YOUR_SERVER_NAME;Database=sims;...
   ```

3. **Run Application**
   ```bash
   dotnet run
   # Access: https://localhost:5001
   ```

4. **Database Initialization**
   - Automatic on first run
   - Sample data seeded
   - Ready for testing

---

## ☁️ Azure App Service Deployment

### Option 1: Using Visual Studio

1. **Right-click Project** → Publish
2. **Select Target** → Azure App Service
3. **Create New** or select existing App Service
4. **Configure Database**:
   - Azure SQL Database or SQL Server
   - Update connection string in Azure Portal
5. **Publish**

### Option 2: Using Azure CLI

```bash
# Create resource group
az group create --name sims-rg --location eastus

# Create App Service plan
az appservice plan create \
  --name sims-plan \
  --resource-group sims-rg \
  --sku B1

# Create App Service
az webapp create \
  --resource-group sims-rg \
  --plan sims-plan \
  --name sims-app

# Deploy from Git
az webapp deployment source config-zip \
  --resource-group sims-rg \
  --name sims-app \
  --src publish.zip
```

### Azure Database Configuration

1. **Create Azure SQL Database**
   ```bash
   az sql server create \
	 --name sims-server \
	 --resource-group sims-rg \
	 --admin-user simsadmin \
	 --admin-password YourPassword123!
   ```

2. **Update appsettings.json**
   ```json
   {
	 "ConnectionStrings": {
	   "DefaultConnection": "Server=tcp:sims-server.database.windows.net,1433;Initial Catalog=sims;Persist Security Info=False;User ID=simsadmin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
	 }
   }
   ```

3. **Configure App Settings**
   - In Azure Portal: App Service → Configuration
   - Add connection string as app setting

---

## 🐳 Docker Deployment

### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SIMS.Web/SIMS.Web.csproj", "SIMS.Web/"]
RUN dotnet restore "SIMS.Web/SIMS.Web.csproj"
COPY . .
WORKDIR "/src/SIMS.Web"
RUN dotnet build "SIMS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMS.Web.dll"]
```

### Build and Run Docker Image

```bash
# Build image
docker build -t sims:latest .

# Run container
docker run -d \
  --name sims-container \
  -p 80:80 \
  -p 443:443 \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=sims;User=sa;Password=YourPassword123!" \
  sims:latest

# Access: http://localhost
```

### Docker Compose

```yaml
version: '3.8'

services:
  sims-app:
	build: .
	ports:
	  - "80:80"
	  - "443:443"
	environment:
	  ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=sims;User=sa;Password=YourPassword123!"
	depends_on:
	  - sqlserver

  sqlserver:
	image: mcr.microsoft.com/mssql/server:2019-latest
	environment:
	  SA_PASSWORD: "YourPassword123!"
	  ACCEPT_EULA: "Y"
	ports:
	  - "1433:1433"
	volumes:
	  - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

---

## 🔐 Security Hardening

### Before Deployment

1. **Update Connection String**
   - Use strong password
   - Use managed identity if on Azure
   - Never commit sensitive data

2. **Configure HTTPS**
   - Install SSL certificate
   - Force HTTPS redirect
   - Set secure cookies

3. **Set Environment Variables**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS=https://+:443;http://+:80
   ```

4. **Update appsettings.Production.json**
   ```json
   {
	 "Logging": {
	   "LogLevel": {
		 "Default": "Warning"
	   }
	 },
	 "AllowedHosts": "yourdomain.com"
   }
   ```

---

## 📊 Performance Optimization

### Database

```sql
-- Create indexes on frequently queried columns
CREATE INDEX idx_user_username ON Users(Username)
CREATE INDEX idx_enrollment_studentid ON Enrollments(StudentId)
CREATE INDEX idx_enrollment_courseid ON Enrollments(CourseId)
CREATE INDEX idx_course_facultyid ON Courses(FacultyId)
```

### Application

1. **Enable Response Compression**
   ```csharp
   services.AddResponseCompression();
   app.UseResponseCompression();
   ```

2. **Configure Caching**
   ```csharp
   services.AddMemoryCache();
   services.AddDistributedMemoryCache();
   ```

3. **Set Production Logging**
   ```json
   {
	 "Logging": {
	   "LogLevel": {
		 "Default": "Error",
		 "Microsoft.AspNetCore": "Error"
	   }
	 }
   }
   ```

---

## 🔄 CI/CD Pipeline

### GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
	branches: [master]

jobs:
  build-and-deploy:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v2

	  - name: Setup .NET
		uses: actions/setup-dotnet@v1
		with:
		  dotnet-version: '8.0.x'

	  - name: Restore
		run: dotnet restore

	  - name: Build
		run: dotnet build --configuration Release

	  - name: Test
		run: dotnet test

	  - name: Publish
		run: dotnet publish -c Release -o published

	  - name: Deploy to Azure
		uses: azure/webapps-deploy@v2
		with:
		  app-name: 'sims-app'
		  publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
		  package: './published'
```

---

## 📈 Monitoring & Logging

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Log Configuration

```json
{
  "Logging": {
	"ApplicationInsights": {
	  "LogLevel": {
		"Default": "Information",
		"Microsoft": "Warning"
	  }
	}
  }
}
```

### Monitor Metrics

- Response time
- Error rate
- User sessions
- Database queries
- CPU/Memory usage

---

## 🆘 Troubleshooting Deployment

### Connection String Issues
```bash
# Test connection
dotnet ef database update --startup-project SIMS.Web
```

### Port Already in Use
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID process_id /F

# Linux/Mac
lsof -i :5001
kill -9 process_id
```

### Database Migration Issues
```bash
# Reset database
dotnet ef database drop --startup-project SIMS.Web
dotnet ef database update --startup-project SIMS.Web
```

### HTTPS Certificate Issues
```bash
# Windows - Trust certificate
dotnet dev-certs https --trust

# Linux/Mac
dotnet dev-certs https --check
```

---

## 📋 Post-Deployment Checklist

- [ ] Application loads successfully
- [ ] Login functionality works
- [ ] Database initialized
- [ ] Sample data accessible
- [ ] HTTPS working
- [ ] Authorization enforced
- [ ] Logging configured
- [ ] Performance acceptable
- [ ] Backups configured
- [ ] Monitoring active

---

## 🔄 Rollback Procedure

```bash
# If deployment fails, revert to previous version
git revert HEAD
dotnet publish -c Release
# Redeploy
```

---

## 📞 Support

For deployment issues:
1. Check logs: `dotnet logs`
2. Review Azure Portal diagnose and solve problems
3. Check connection string
4. Verify database accessibility
5. Review error messages in application insights

---

## 🎉 Deployment Complete

Once deployed:
- Access application at: `https://yourdomain.com`
- Monitor performance in Azure Portal
- Configure automated backups
- Set up alerts for errors
- Monitor user activity

---

**Happy Deploying! 🚀**
