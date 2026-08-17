IF NOT EXISTS (SELECT 1 FROM dbo.Tickets)
BEGIN
    WITH
    E1(N) AS (
        SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL 
        SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL 
        SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
    ), -- 10 rows
    E2(N) AS (SELECT 1 FROM E1 AS A CROSS JOIN E1 AS B), -- 100 rows
    E3(N) AS (SELECT 1 FROM E2 AS A CROSS JOIN E1 AS B), -- 1,000 rows
    Tally AS (
        SELECT TOP (1000)
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Idx
        FROM E3
    ),
    CalendarMath AS (
        SELECT
            Idx,
            (Idx / 10) AS DayIndex,        -- 0 to 99 (100 weekdays total)
            (Idx % 10) AS TicketOfDay,     -- 0 to 9 per weekday
            -- Deterministic pseudo-random integer based on row index
            ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(Idx AS VARCHAR(10))))) AS Seed
        FROM Tally
    ),
    CalculatedData AS (
        SELECT
            C.Idx,
            C.Seed,
            -- Calculate weekday date: starts Monday 2026-01-05, skips weekends
            DATEADD(DAY, (C.DayIndex / 5) * 7 + (C.DayIndex % 5), CAST('2026-01-05' AS DATE)) AS TicketDate,
            -- Seconds offset between 09:00:00 (32,400s) and 17:00:00 (61,200s) -> 28,800s window
            DATEADD(SECOND, 32400 + (C.Seed % 28801), CAST(DATEADD(DAY, (C.DayIndex / 5) * 7 + (C.DayIndex % 5), CAST('2026-01-05' AS DATE)) AS DATETIME2)) AS CreatedAt,
            -- Status lookup
            CASE (C.Seed / 7) % 4
                WHEN 0 THEN N'Open'
                WHEN 1 THEN N'InProgress'
                WHEN 2 THEN N'Resolved'
                WHEN 3 THEN N'Closed'
            END AS Status,
            -- Priority lookup
            CASE (C.Seed / 13) % 4
                WHEN 0 THEN N'Low'
                WHEN 1 THEN N'Medium'
                WHEN 2 THEN N'High'
                WHEN 3 THEN N'Critical'
            END AS Priority,
            -- AssignedTo lookup
            CASE (C.Seed / 19) % 5
                WHEN 0 THEN N'Alex'
                WHEN 1 THEN N'Jordan'
                WHEN 2 THEN N'Sam'
                WHEN 3 THEN N'Chris'
                WHEN 4 THEN N'Taylor'
            END AS AssignedTo,
            -- Issue template index
            (C.Seed / 23) % 20 AS IssueIdx
        FROM CalendarMath C
    ),
    Issues AS (
        SELECT IssueIdx, Title, Description
        FROM (VALUES
            (0,  N'Cannot sign in', N'User receives an invalid password message after credential reset.'),
            (1,  N'Export is slow', N'CSV export takes more than one minute on large data sets.'),
            (2,  N'Update help text', N'Change wording on the user account configuration page.'),
            (3,  N'Application crashes on startup', N'Application terminates unexpectedly following authentication.'),
            (4,  N'Unable to upload documents', N'PDF upload returns HTTP 500 internal server error.'),
            (5,  N'Profile picture not saving', N'Image updates do not persist after page refresh.'),
            (6,  N'Dashboard widgets overlap', N'Grid layout renders improperly on sub-1080p viewport resolutions.'),
            (7,  N'Search returns incorrect results', N'Querying by customer name produces duplicate records.'),
            (8,  N'Email notifications delayed', N'Outbound SMTP notifications delayed by approximately 20 minutes.'),
            (9,  N'Session expires too quickly', N'Active sessions expire prematurely after five minutes of use.'),
            (10, N'Audit history missing', N'Change log history grid displays empty records.'),
            (11, N'Mobile menu not responsive', N'Navigation drawer fails to open on mobile viewports.'),
            (12, N'Customer import fails', N'Batch CSV processing terminates with a gateway timeout.'),
            (13, N'Incorrect invoice total', N'Tax rounding logic causes discrepancies in checkout calculation.'),
            (14, N'Dark mode styling issue', N'Action buttons lack adequate contrast against dark backgrounds.'),
            (15, N'Password reset email missing', N'Automated recovery emails fail to reach external domains.'),
            (16, N'API response is slow', N'Latency on core reporting endpoints exceeds 3,000ms threshold.'),
            (17, N'Browser compatibility issue', N'Application renders a blank viewport on Safari versions < 17.'),
            (18, N'Footer links broken', N'Privacy policy hyperlink returns HTTP 404 Not Found.'),
            (19, N'User cannot change password', N'Save button returns validation error on special characters.')
        ) AS T(IssueIdx, Title, Description)
    )
    INSERT INTO dbo.Tickets (Title, Description, Status, Priority, AssignedTo, CreatedAt)
    SELECT 
        I.Title,
        I.Description,
        D.Status,
        D.Priority,
        D.AssignedTo,
        D.CreatedAt
    FROM CalculatedData D
    INNER JOIN Issues I ON D.IssueIdx = I.IssueIdx
    ORDER BY D.CreatedAt;
END
GO