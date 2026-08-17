-- Azure SQL Database: create Tickets table
-- Maps TicketTest.Api/Models/Ticket.cs and AppDbContext conventions.

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets
    (
        Id          INT            NOT NULL IDENTITY(1, 1),
        Title       NVARCHAR(150)  NOT NULL,
        Description NVARCHAR(MAX)  NOT NULL,
        Status      NVARCHAR(32)   NOT NULL CONSTRAINT DF_Tickets_Status DEFAULT (N'Open'),
        Priority    NVARCHAR(32)   NOT NULL CONSTRAINT DF_Tickets_Priority DEFAULT (N'Medium'),
        AssignedTo  NVARCHAR(100)  NULL,
        CreatedAt   DATETIME2(7)   NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2(7)   NULL,
        Version     INT            NOT NULL CONSTRAINT DF_Tickets_Version DEFAULT (1),

        CONSTRAINT PK_Tickets PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Tickets_Status CHECK (Status IN (N'Open', N'InProgress', N'Resolved', N'Closed')),
        CONSTRAINT CK_Tickets_Priority CHECK (Priority IN (N'Low', N'Medium', N'High', N'Critical'))
    );
END
GO
