-- Create ChatbotDB database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ChatbotDB')
BEGIN
    CREATE DATABASE ChatbotDB;
    PRINT 'Database ChatbotDB created successfully';
END
ELSE
BEGIN
    PRINT 'Database ChatbotDB already exists';
END
GO

-- Switch to ChatbotDB
USE ChatbotDB;
GO

-- Wait for Entity Framework migrations to create tables
-- This script will create sample application users after tables exist

-- Insert sample application users
PRINT 'Creating sample application users...';

-- Check if Users table exists before inserting data
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    -- Insert demo users if they don't exist
    IF NOT EXISTS (SELECT * FROM Users WHERE Id = 'demo-user-1')
    BEGIN
        INSERT INTO Users (Id, Name, Email) 
        VALUES ('demo-user-1', 'Demo User One', 'demo1@example.com');
        PRINT 'Created demo user: demo1@example.com';
    END

    IF NOT EXISTS (SELECT * FROM Users WHERE Id = 'demo-user-2')
    BEGIN
        INSERT INTO Users (Id, Name, Email) 
        VALUES ('demo-user-2', 'Demo User Two', 'demo2@example.com');
        PRINT 'Created demo user: demo2@example.com';
    END

    IF NOT EXISTS (SELECT * FROM Users WHERE Id = 'admin-user')
    BEGIN
        INSERT INTO Users (Id, Name, Email) 
        VALUES ('admin-user', 'Admin User', 'admin@example.com');
        PRINT 'Created admin user: admin@example.com';
    END

    IF NOT EXISTS (SELECT * FROM Users WHERE Id = 'test-user')
    BEGIN
        INSERT INTO Users (Id, Name, Email) 
        VALUES ('test-user', 'Test User', 'test@example.com');
        PRINT 'Created test user: test@example.com';
    END

    PRINT 'Sample application users created successfully';
END
ELSE
BEGIN
    PRINT 'Users table does not exist yet. Run Entity Framework migrations first.';
    PRINT 'After migrations, you can run this script again to create sample users.';
END
GO

PRINT 'Application user initialization completed';