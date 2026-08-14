-- Senior C# Developer Assessment - SQL Exercise
-- The starter application uses SQLite. Submit executable SQLite-compatible SQL.
-- Where SQL Server syntax or indexing strategy differs, include the SQL Server equivalent as a comment.

-- 1. Return all active tickets (Open or InProgress) with High or Critical priority,
--    ordered by Priority (Critical first) and then CreatedAt newest first.

-- 2. Return ticket counts by Status and Priority, and include the percentage that each
--    Priority represents within its Status. Return the percentage to two decimal places.
--    Example columns: Status, Priority, TicketCount, PercentageWithinStatus.

-- 3. Return each assignee who has more than two active tickets (Open or InProgress).
--    Include: AssignedTo, ActiveTicketCount, CriticalTicketCount and OldestActiveTicketCreatedAt.
--    Exclude unassigned tickets.

-- 4. Return the most recently created ticket for each assignee.
--    If an assignee has multiple tickets with the same CreatedAt, return the ticket with the highest Id.
--    Exclude unassigned tickets. A window function is acceptable.

-- 5. Return tickets that are considered stale. A ticket is stale when:
--      - it is not Closed, and
--      - the most recent activity date (UpdatedAt when present, otherwise CreatedAt)
--        is more than 30 days old.
--    Return Id, Title, Status, AssignedTo and the calculated LastActivityAt.

-- 6. Write a single safe UPDATE statement that changes ticket ID 1 to Resolved and sets
--    UpdatedAt to the current UTC timestamp, but only when the ticket is currently Open or InProgress.
--    The statement must not modify an already Closed ticket.

-- 7. The API frequently executes the following query:
--      SELECT *
--      FROM Tickets
--      WHERE Status = 'Open' AND Priority = 'Critical'
--      ORDER BY CreatedAt DESC;
--    Propose an index for this access pattern. Write the CREATE INDEX statement and briefly explain
--    the column order. Also state whether you would make the same choice in SQL Server and why/why not.

-- 8. Return the assignee with the highest percentage of Critical tickets among all of their tickets.
--    Only consider assignees with at least three assigned tickets. Handle division correctly so the
--    percentage is not truncated by integer division. Return AssignedTo, TotalTickets,
--    CriticalTickets and CriticalPercentage.

-- 9. A reporting query filters by Status and CreatedAt but currently uses:
--      WHERE date(CreatedAt) >= date('now', '-30 day')
--    Explain why applying a function to an indexed date column can be undesirable in SQL Server,
--    and rewrite the predicate in a more index-friendly form. Provide both SQLite-compatible SQL
--    and the SQL Server equivalent as a comment.
