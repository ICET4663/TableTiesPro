using Microsoft.EntityFrameworkCore.Migrations;
using System; // Needed for Guid

#nullable disable

namespace TableTies.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Define the GUIDs to use for consistent seeding across tables
            // These need to be defined within the Up method to be accessible
            var org1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var org2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var rest1Id = Guid.Parse("aaaaaaa1-0000-0000-0000-000000000001");
            var rest2Id = Guid.Parse("aaaaaaa2-0000-0000-0000-000000000002");
            var rest3Id = Guid.Parse("aaaaaaa3-0000-0000-0000-000000000003");

            var table1Id = Guid.Parse("bbbbbbb1-0000-0000-0000-000000000001");
            var table2Id = Guid.Parse("bbbbbbb2-0000-0000-0000-000000000002");
            var table3Id = Guid.Parse("bbbbbbb3-0000-0000-0000-000000000003");
            var table4Id = Guid.Parse("bbbbbbb4-0000-0000-0000-000000000004");
            var table5Id = Guid.Parse("bbbbbbb5-0000-0000-0000-000000000005");

            // Define GUIDs for your seeded consultants (you can generate these once)
            var consultant1Id = Guid.Parse("c0000001-0000-0000-0000-000000000001");
            var consultant2Id = Guid.Parse("c0000002-0000-0000-0000-000000000002");
            // Add more consultant GUIDs here if needed

            // Define placeholder GUIDs for users to link bookings to.
            // REPLACE THESE WITH ACTUAL USER IDs FROM YOUR DATABASE OR SEEDED USER IDs.
            var user1Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"); // <-- REPLACE with a valid User ID
            var user2Id = Guid.Parse("d0000002-0000-0000-0000-000000000002"); // <-- REPLACE with another valid User ID
            // Add more user GUIDs if needed for bookings

            // ===============================================================
            // INSERT SEED DATA
            // Use migrationBuilder.InsertData to add rows to tables
            // ===============================================================

            // Insert Organizations
            migrationBuilder.InsertData(
                table: "Organizations", // Table name
                columns: new[] { "Id", "Name", "Address", "Phone", "Description" }, // Column names
                values: new object[,] // Array of rows to insert
                {
                    { org1Id, "FoodCorp", "123 Food St, Cityville", "123-456-7890", "Leading food and beverage organization" },
                    { org2Id, "DineWell", "456 Dine Ave, Townsville", "987-654-3210", "Premier dining experiences" }
                });

            // Insert Restaurants
            migrationBuilder.InsertData(
                table: "Restaurants", // Table name
                columns: new[] { "Id", "Name", "Location", "OrganizationId" }, // Column names
                values: new object[,] // Array of rows to insert
                {
                    { rest1Id, "Foodie Haven", "Downtown", org1Id }, // Link to org1Id
                    { rest2Id, "Gourmet Grill", "Uptown", org1Id }, // Link to org1Id
                    { rest3Id, "Healthy Eats", "Suburbs", org2Id }  // Link to org2Id
                });

            // Insert RestaurantTables
            migrationBuilder.InsertData(
                table: "RestaurantTables", // Table name
                columns: new[] { "Id", "TableName", "Capacity", "RestaurantId" }, // Column names
                values: new object[,] // Array of rows to insert
                {
                    { table1Id, "Table 1", 4, rest1Id }, // Link to rest1Id
                    { table2Id, "Table 2", 4, rest1Id }, // Link to rest1Id
                    { table3Id, "Table 3", 2, rest2Id }, // Link to rest2Id
                    { table4Id, "Table 4", 6, rest2Id }, // Link to rest2Id
                    { table5Id, "Table 5", 2, rest3Id }  // Link to rest3Id
                });

            // Insert Consultants
            migrationBuilder.InsertData(
                table: "Consultants", // Table name
                columns: new[] { "Id", "Name", "Specialty" }, // Columns to insert data into
                values: new object[,] // Array of rows to insert
                {
                    { consultant1Id, "Dr. Anya Sharma", "AI Ethics" },
                    { consultant2Id, "Mr. Ben Carter", "Cloud Migration" }
                    // Add more consultant data here
                });

            // Insert Consultant Bookings (Requires valid User IDs)
            // Make sure the User IDs used here exist in your AspNetUsers table
            migrationBuilder.InsertData(
                table: "ConsultantBookings", // Table name
                columns: new[] { "ConsultantId", "UserId", "BookingDateTime", "Duration", "Details", "CancelledDateTime" }, // Columns
                values: new object[,] // Array of rows
                {
                    // Example booking for Consultant 1 by User 1
                    { consultant1Id, user1Id, new DateTime(2025, 5, 20, 10, 0, 0), new TimeSpan(1, 0, 0), "Discuss AI ethics in healthcare.", null },
                    // Example booking for Consultant 2 by User 1
                    { consultant2Id, user1Id, new DateTime(2025, 5, 22, 14, 30, 0), new TimeSpan(0, 45, 0), "Review cloud migration strategy.", null },
                    // Example booking for Consultant 1 by User 2
                    { consultant1Id, user2Id, new DateTime(2025, 6, 5, 9, 0, 0), new TimeSpan(1, 30, 0), "Follow-up on ethical guidelines.", null }
                    // Add more consultant booking data here
                });


            // TODO: Add InsertData for Hotels, RoomBookings, TableBookings if needed
            // Note: TableBookings and RoomBookings might require existing User IDs if not nullable.
            // You might need to seed a default user or handle this differently.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Define the GUIDs to use for consistent seeding
            // These also need to be defined within the Down method to be accessible
            var org1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var org2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var rest1Id = Guid.Parse("aaaaaaa1-0000-0000-0000-000000000001");
            var rest2Id = Guid.Parse("aaaaaaa2-0000-0000-0000-000000000002");
            var rest3Id = Guid.Parse("aaaaaaa3-0000-0000-0000-000000000003");

            var table1Id = Guid.Parse("bbbbbbb1-0000-0000-0000-000000000001");
            var table2Id = Guid.Parse("bbbbbbb2-0000-0000-0000-000000000002");
            var table3Id = Guid.Parse("bbbbbbb3-0000-0000-0000-000000000003");
            var table4Id = Guid.Parse("bbbbbbb4-0000-0000-0000-000000000004");
            var table5Id = Guid.Parse("bbbbbbb5-0000-0000-0000-000000000005");

            // Define GUIDs for your seeded consultants
            var consultant1Id = Guid.Parse("c0000001-0000-0000-0000-000000000001");
            var consultant2Id = Guid.Parse("c0000002-0000-0000-0000-000000000002");
            // Add more consultant GUIDs here if needed

            // Define placeholder GUIDs for users (must match those in Up)
            var user1Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"); // <-- Must match User ID in Up
            var user2Id = Guid.Parse("d0000002-0000-0000-0000-000000000002"); // <-- Must match User ID in Up
            // Add more user GUIDs if needed for bookings

            // ===============================================================
            // DELETE SEED DATA
            // Use migrationBuilder.DeleteData to remove rows from tables
            // This should reverse the operations in the Up method
            // Delete in reverse order of insertion to respect foreign key constraints
            // ===============================================================

            // Delete ConsultantBookings data
            // Deleting by composite key (ConsultantId, UserId, BookingDateTime)
            migrationBuilder.DeleteData(
                table: "ConsultantBookings",
                keyColumns: new[] { "ConsultantId", "UserId", "BookingDateTime" },
                keyValues: new object[,]
                {
                     { consultant1Id, user1Id, new DateTime(2025, 5, 20, 10, 0, 0) },
                     { consultant2Id, user1Id, new DateTime(2025, 5, 22, 14, 30, 0) },
                     { consultant1Id, user2Id, new DateTime(2025, 6, 5, 9, 0, 0) }
                     // Add key values for any other consultant bookings you seeded
                });
            // Note: If deleting by composite key is problematic due to data changes or uniqueness,
            // you might consider clearing the table for development purposes:
            // migrationBuilder.Sql("DELETE FROM ConsultantBookings;"); // Use raw SQL if needed


            // Delete Consultants data
            migrationBuilder.DeleteData(
                table: "Consultants", // Table name
                keyColumn: "Id", // Primary key column
                keyValues: new object[] { consultant1Id, consultant2Id }); // Array of IDs to delete
            // Add more consultant IDs here if needed


            // Delete RestaurantTables
            migrationBuilder.DeleteData(
                table: "RestaurantTables", // Table name
                keyColumn: "Id", // Column to match for deletion
                keyValues: new object[] { table1Id, table2Id, table3Id, table4Id, table5Id }); // Array of IDs to delete

            // Delete Restaurants
            migrationBuilder.DeleteData(
                table: "Restaurants", // Table name
                keyColumn: "Id", // Column to match for deletion
                keyValues: new object[] { rest1Id, rest2Id, rest3Id }); // Array of IDs to delete

            // Delete Organizations
            migrationBuilder.DeleteData(
                table: "Organizations", // Table name
                keyColumn: "Id", // Column to match for deletion
                keyValues: new object[] { org1Id, org2Id }); // Array of IDs to delete

            // TODO: Add DeleteData for Hotels, RoomBookings, TableBookings if needed
            // If you seeded users in this migration, delete them here last.
            // migrationBuilder.DeleteData(
            //     table: "AspNetUsers",
            //     keyColumn: "Id",
            //     keyValues: new object[] { user1Id, user2Id }); // Add more user IDs if needed
        }
    }
}
