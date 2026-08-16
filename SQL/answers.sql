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
  AND COALESCE(UpdatedAt, CreatedAt) < datetime('now', '-30 days');


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
CREATE INDEX IX_Tickets_Status_Priority_CreatedAt
ON Tickets (Status, Priority, CreatedAt DESC);

-- Explanation:
-- Status and Priority are equality predicates, so they are the leading index keys.
-- CreatedAt follows them because it supports the requested ordering after the
-- equality predicates have identified the relevant rows. This can avoid an
-- additional sort operation.
--
-- SQL Server:
-- I would use the same key-column order in SQL Server:
-- CREATE INDEX IX_Tickets_Status_Priority_CreatedAt
-- ON Tickets (Status, Priority, CreatedAt DESC);
--
-- Because the query uses SELECT *, this index does not necessarily cover every
-- selected column. In SQL Server I would review the actual execution plan,
-- lookup cost and workload before adding INCLUDE columns. Including every Ticket
-- column merely to cover SELECT * could make the index unnecessarily wide and
-- increase storage and write costs.

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
SELECT *
FROM Tickets
WHERE Status = 'Open'
  AND CreatedAt >= datetime('now', '-30 days');

-- SQL Server equivalent:
-- SELECT *
-- FROM Tickets
-- WHERE Status = 'Open'
--   AND CreatedAt >= DATEADD(day, -30, SYSUTCDATETIME());

-- Explanation:
-- Applying a function such as date(CreatedAt) to an indexed column makes the
-- predicate non-SARGable. In SQL Server this can prevent an efficient index seek
-- because the database may need to evaluate the function for rows before applying
-- the predicate.
--
-- Comparing the raw CreatedAt column against a calculated boundary leaves the
-- indexed column unchanged, making the predicate more amenable to an index seek.