-- Script tạo SQL login cho remote access
-- Chạy script này trong SQL Server Management Studio hoặc sqlcmd

-- 1. Tạo login mới
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'remote_user')
BEGIN
    CREATE LOGIN remote_user WITH PASSWORD = 'Remote@123456';
    PRINT 'Login remote_user created successfully';
END
ELSE
BEGIN
    PRINT 'Login remote_user already exists';
END

-- 2. Cấp quyền cho database sims1
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
