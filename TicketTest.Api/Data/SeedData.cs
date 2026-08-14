using TicketTest.Api.Models;

namespace TicketTest.Api.Data;

public static class SeedData
{
    public static void Initialise(AppDbContext db)
    {
        if (db.Tickets.Any())
            return;

        db.Tickets.AddRange(

            new Ticket
            {
                Title = "Cannot sign in",
                Description = "User receives an invalid password message.",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Alex"
            },

            new Ticket
            {
                Title = "Export is slow",
                Description = "CSV export takes more than one minute.",
                Status = "InProgress",
                Priority = "Medium",
                AssignedTo = "Jordan"
            },

            new Ticket
            {
                Title = "Update help text",
                Description = "Change wording on the account page.",
                Status = "Resolved",
                Priority = "Low",
                AssignedTo = "Sam"
            },

            new Ticket
            {
                Title = "Application crashes on startup",
                Description = "Application crashes after login.",
                Status = "Open",
                Priority = "Critical",
                AssignedTo = "Chris"
            },

            new Ticket
            {
                Title = "Unable to upload documents",
                Description = "PDF upload fails with 500 error.",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Taylor"
            },

            new Ticket
            {
                Title = "Profile picture not saving",
                Description = "Changes disappear after refresh.",
                Status = "InProgress",
                Priority = "Medium",
                AssignedTo = "Alex"
            },

            new Ticket
            {
                Title = "Dashboard widgets overlap",
                Description = "Occurs on smaller screens.",
                Status = "Resolved",
                Priority = "Medium",
                AssignedTo = "Jordan"
            },

            new Ticket
            {
                Title = "Search returns incorrect results",
                Description = "Searching by customer name gives duplicates.",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Chris"
            },

            new Ticket
            {
                Title = "Email notifications delayed",
                Description = "Emails arrive 20 minutes late.",
                Status = "Closed",
                Priority = "Medium",
                AssignedTo = "Sam"
            },

            new Ticket
            {
                Title = "Session expires too quickly",
                Description = "Users are logged out after five minutes.",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Taylor"
            },

            new Ticket
            {
                Title = "Audit history missing",
                Description = "History tab is empty.",
                Status = "InProgress",
                Priority = "Low",
                AssignedTo = "Jordan"
            },

            new Ticket
            {
                Title = "Mobile menu not responsive",
                Description = "Hamburger menu cannot be opened.",
                Status = "Resolved",
                Priority = "Low",
                AssignedTo = "Alex"
            },

            new Ticket
            {
                Title = "Customer import fails",
                Description = "Large CSV files timeout.",
                Status = "Open",
                Priority = "Critical",
                AssignedTo = "Chris"
            },

            new Ticket
            {
                Title = "Incorrect invoice total",
                Description = "Tax calculation is incorrect.",
                Status = "InProgress",
                Priority = "Critical",
                AssignedTo = "Sam"
            },

            new Ticket
            {
                Title = "Dark mode styling issue",
                Description = "Buttons are unreadable.",
                Status = "Closed",
                Priority = "Low",
                AssignedTo = "Taylor"
            },

            new Ticket
            {
                Title = "Password reset email missing",
                Description = "Users never receive reset email.",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Alex"
            },

            new Ticket
            {
                Title = "API response is slow",
                Description = "Average response exceeds three seconds.",
                Status = "InProgress",
                Priority = "Critical",
                AssignedTo = "Chris"
            },

            new Ticket
            {
                Title = "Browser compatibility issue",
                Description = "Safari displays blank page.",
                Status = "Resolved",
                Priority = "Medium",
                AssignedTo = "Jordan"
            },

            new Ticket
            {
                Title = "Footer links broken",
                Description = "Privacy Policy link returns 404.",
                Status = "Closed",
                Priority = "Low",
                AssignedTo = "Sam"
            },

            new Ticket
            {
                Title = "User cannot change password",
                Description = "Save button returns validation error.",
                Status = "Open",
                Priority = "Medium",
                AssignedTo = "Taylor"
            }
        );

        db.SaveChanges();
    }
}