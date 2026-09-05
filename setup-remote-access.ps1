# Script cấu hình SQL Server Remote Access
# Chạy với quyền Administrator

Write-Host "=== Cấu hình SQL Server Remote Access ===" -ForegroundColor Green

# 1. Mở firewall port 1433
Write-Host "`n1. Mở firewall port 1433..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName 'SQL Server (TCP 1433)' -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
Write-Host "   ✓ Firewall đã được cấu hình" -ForegroundColor Green

# 2. Kiểm tra SQL Server đang chạy
Write-Host "`n2. Kiểm tra SQL Server..." -ForegroundColor Yellow
$service = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
if ($service -and $service.Status -eq "Running") {
    Write-Host "   ✓ SQL Server đang chạy" -ForegroundColor Green
} else {
    Write-Host "   ✗ SQL Server không chạy!" -ForegroundColor Red
    Write-Host "   Vui lòng khởi động SQL Server Service" -ForegroundColor Red
    exit 1
}

# 3. Tạo script SQL tạo login
Write-Host "`n3. Tạo SQL login cho remote access..." -ForegroundColor Yellow
$sqlScript = @"
-- Tạo login cho remote access
-- Chạy script này trong SQL Server Management Studio hoặc sqlcmd

-- Tạo login mới
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'remote_user')
BEGIN
    CREATE LOGIN remote_user WITH PASSWORD = 'Remote@123456';
    PRINT 'Login remote_user created successfully';
END
ELSE
BEGIN
    PRINT 'Login remote_user already exists';
END

-- Cấp quyền cho database sims1
USE sims1;

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'remote_user')
BEGIN
    CREATE USER remote_user FOR LOGIN remote_user;
    ALTER ROLE db_owner ADD MEMBER remote_user;
    PRINT 'User remote_user added to sims1 database';
END
ELSE
BEGIN
    PRINT 'User remote_user already exists in sims1';
END
"@

$sqlScript | Out-File -FilePath "create-remote-user.sql" -Encoding UTF8
Write-Host "   ✓ File create-remote-user.sql đã được tạo" -ForegroundColor Green

# 4. Hiển thị IP của máy
Write-Host "`n4. IP Address của máy bạn:" -ForegroundColor Yellow
$ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -ne "127.0.0.1"} | Select-Object -First 1).IPAddress
Write-Host "   $ip" -ForegroundColor Cyan

Write-Host "`n=== Hoàn thành! ===" -ForegroundColor Green
Write-Host "`nBước tiếp theo:" -ForegroundColor Yellow
Write-Host "1. Chạy file create-remote-user.sql trong SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Cung cấp IP và thông tin đăng nhập cho người khác" -ForegroundColor White
Write-Host "3. Họ sẽ dùng connection string:" -ForegroundColor White
Write-Host "   Server=tcp:$ip,1433;Database=sims1;User ID=remote_user;Password=Remote@123456;Encrypt=True;TrustServerCertificate=False;" -ForegroundColor Cyan
