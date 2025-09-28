-- Sample data script - run after Entity Framework migrations
-- This script creates sample application users and conversations

USE ChatbotDB;
GO

PRINT 'Inserting sample application data...';

-- Insert sample users
MERGE Users AS target
USING (VALUES 
    ('demo-user-1', 'Alice Johnson', 'alice.johnson@example.com'),
    ('demo-user-2', 'Bob Smith', 'bob.smith@example.com'),
    ('admin-user', 'Admin User', 'admin@chatbot.com'),
    ('test-user', 'Test User', 'test@example.com')
) AS source (Id, Name, Email)
ON target.Id = source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, Name, Email)
    VALUES (source.Id, source.Name, source.Email);

PRINT 'Sample users created/updated';

-- Insert sample conversations (only if Users table has data)
IF EXISTS (SELECT * FROM Users WHERE Id = 'demo-user-1')
BEGIN
    -- Insert sample conversation for demo-user-1
    IF NOT EXISTS (SELECT * FROM Conversations WHERE Title = 'Welcome Chat' AND UserId = 'demo-user-1')
    BEGIN
        INSERT INTO Conversations (Title, UserId, Status, Timestamp)
        VALUES ('Welcome Chat', 'demo-user-1', 1, GETUTCDATE()); -- Status 1 = Completed
        
        DECLARE @ConversationId INT = SCOPE_IDENTITY();
        
        -- Insert sample messages for this conversation
        INSERT INTO Messages (ConversationId, Role, Content, Timestamp)
        VALUES 
            (@ConversationId, 0, 'Hello! I''m new to this chatbot. How can it help me?', GETUTCDATE()),
            (@ConversationId, 1, 'Welcome! I''m an AI assistant that can help you with various tasks like answering questions, providing information, helping with writing, and much more. What would you like to know or discuss?', DATEADD(SECOND, 2, GETUTCDATE())),
            (@ConversationId, 0, 'That sounds great! Can you help me understand how to use this service effectively?', DATEADD(SECOND, 10, GETUTCDATE())),
            (@ConversationId, 1, 'Absolutely! Here are some tips for getting the most out of our conversation: 1) Be specific with your questions, 2) Feel free to ask follow-up questions, 3) I can help with creative tasks, analysis, explanations, and problem-solving. What specific area would you like to explore first?', DATEADD(SECOND, 15, GETUTCDATE()));
        
        PRINT 'Sample conversation created for demo-user-1';
    END

    -- Insert sample conversation for demo-user-2
    IF NOT EXISTS (SELECT * FROM Conversations WHERE Title = 'Programming Help' AND UserId = 'demo-user-2')
    BEGIN
        INSERT INTO Conversations (Title, UserId, Status, Timestamp)
        VALUES ('Programming Help', 'demo-user-2', 1, DATEADD(HOUR, -2, GETUTCDATE()));
        
        DECLARE @ConversationId2 INT = SCOPE_IDENTITY();
        
        INSERT INTO Messages (ConversationId, Role, Content, Timestamp)
        VALUES 
            (@ConversationId2, 0, 'I''m learning C# and Entity Framework. Can you explain what migrations are?', DATEADD(HOUR, -2, GETUTCDATE())),
            (@ConversationId2, 1, 'Entity Framework migrations are a way to incrementally update your database schema to keep it in sync with your data model. They allow you to version control your database changes and apply them in a consistent way across different environments. Would you like me to explain how to create and run migrations?', DATEADD(HOUR, -2, DATEADD(SECOND, 3, GETUTCDATE()))),
            (@ConversationId2, 0, 'Yes, please! That would be very helpful.', DATEADD(HOUR, -2, DATEADD(SECOND, 8, GETUTCDATE())));
        
        PRINT 'Sample conversation created for demo-user-2';
    END
END

PRINT 'Sample application data insertion completed successfully!';
GO