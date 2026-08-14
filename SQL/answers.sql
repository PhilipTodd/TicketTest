-- Senior C# Developer Assessment - SQL Answers
-- Candidate: 
-- Please place each answer directly below the matching question number.

-- 1.
SELECT *
FROM Tickets
WHERE Status IN ('Open', 'InProgress')
  AND Priority IN ('High', 'Critical')
ORDER BY
  CASE Priority
    WHEN 'Critical' THEN 1
    WHEN 'High' THEN 2
    ELSE 3
  END,
  CreatedAt DESC;


-- 2.
SELECT
  Status,
  Priority,
  COUNT(*) AS TicketCount,
  ROUND(100.0 * COUNT(*) / SUM(COUNT(*)) OVER (PARTITION BY Status), 2) AS PercentageWithinStatus
FROM Tickets
GROUP BY Status, Priority
ORDER BY Status, Priority;


-- 3.
SELECT
  AssignedTo,
  COUNT(*) AS ActiveTicketCount,
  SUM(CASE WHEN Priority = 'Critical' THEN 1 ELSE 0 END) AS CriticalTicketCount,
  MIN(CreatedAt) AS OldestActiveTicketCreatedAt
FROM Tickets
WHERE Status IN ('Open', 'InProgress')
  AND AssignedTo IS NOT NULL
GROUP BY AssignedTo
HAVING COUNT(*) > 2;


-- 4.
SELECT
  Id,
  Title,
  Description,
  Status,
  Priority,
  AssignedTo,
  CreatedAt,
  UpdatedAt
FROM (
  SELECT
    *,
    ROW_NUMBER() OVER (
      PARTITION BY AssignedTo
      ORDER BY CreatedAt DESC, Id DESC
    ) AS rn
  FROM Tickets
  WHERE AssignedTo IS NOT NULL
) ranked
WHERE rn = 1;


-- 5.
SELECT
  Id,
  Title,
  Status,
  AssignedTo,
  COALESCE(UpdatedAt, CreatedAt) AS LastActivityAt
FROM Tickets
WHERE Status <> 'Closed'
  AND datetime(COALESCE(UpdatedAt, CreatedAt)) < datetime('now', '-30 days');


-- 6.
UPDATE Tickets
SET Status = 'Resolved',
    UpdatedAt = datetime('now')
WHERE Id = 1
  AND Status IN ('Open', 'InProgress');
-- SQL Server equivalent:
-- UPDATE Tickets
-- SET Status = 'Resolved',
--     UpdatedAt = GETUTCDATE()
-- WHERE Id = 1
--   AND Status IN ('Open', 'InProgress');


-- 7.
-- SQL:
CREATE INDEX IX_Tickets_Status_Priority_CreatedAt
ON Tickets (Status, Priority, CreatedAt DESC);

-- Explanation:
-- Status and Priority are equality predicates, so they belong first in the key.
-- CreatedAt supports the ORDER BY CreatedAt DESC, allowing the engine to seek matching
-- rows and return them in sort order without a separate sort step.
-- SQL Server: Yes, the same column order applies. SQL Server also benefits from leading
-- equality columns followed by the sort column. DESC on CreatedAt can be specified
-- explicitly in SQL Server 2019+ (CREATE INDEX ... (CreatedAt DESC)); older versions
-- still use the index efficiently via a backward scan.

-- 8.
SELECT
  AssignedTo,
  TotalTickets,
  CriticalTickets,
  CriticalPercentage
FROM (
  SELECT
    AssignedTo,
    COUNT(*) AS TotalTickets,
    SUM(CASE WHEN Priority = 'Critical' THEN 1 ELSE 0 END) AS CriticalTickets,
    ROUND(100.0 * SUM(CASE WHEN Priority = 'Critical' THEN 1 ELSE 0 END) / COUNT(*), 2) AS CriticalPercentage
  FROM Tickets
  WHERE AssignedTo IS NOT NULL
  GROUP BY AssignedTo
  HAVING COUNT(*) >= 3
) assignee_stats
ORDER BY CriticalPercentage DESC, AssignedTo
LIMIT 1;


-- 9.
-- SQLite:
-- WHERE CreatedAt >= datetime('now', '-30 days')
-- (Compare the raw column to a computed boundary instead of wrapping CreatedAt in date().)

-- SQL Server equivalent:
-- WHERE CreatedAt >= DATEADD(day, -30, SYSUTCDATETIME())

-- Explanation:
-- Applying a function such as date(CreatedAt) to an indexed column makes the predicate
-- non-sargable: SQL Server must evaluate the function on every row instead of seeking
-- the index on CreatedAt. Rewriting the filter to compare CreatedAt directly against a
-- precomputed cutoff preserves index usability and typically performs much better on
-- large tables.
