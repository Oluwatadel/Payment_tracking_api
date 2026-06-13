using BC = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using PaymentTracker.Models;

namespace PaymentTracker.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext context, IConfiguration configuration, ILogger logger)
        {
            var adminUsername = configuration["ADMIN_USERNAME"];
            var adminPhone = configuration["ADMIN_PHONE"];
            var adminPassword = configuration["ADMIN_PASSWORD"];

            if (string.IsNullOrWhiteSpace(adminUsername) ||
                string.IsNullOrWhiteSpace(adminPhone) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("Admin seed skipped because one or more required environment variables are missing: ADMIN_USERNAME, ADMIN_PHONE, ADMIN_PASSWORD.");
                    return;
            }
            var admin = await context.Users
                   .Include(u => u.Account)
                   .FirstOrDefaultAsync(u => u.Username == adminUsername || u.PhoneNumber == adminPhone);

            if (admin == null)
            {
                admin = new User
                {
                    Username = adminUsername,
                    PhoneNumber = adminPhone,
                    PasswordHash = BC.HashPassword(adminPassword),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                admin.Account = new Account
                {
                    UserId = admin.Id,
                    BankName = "Default Bank",
                    AccountNumber = $"ACC-{admin.Id:N}".Substring(0, 16),
                    AccountHolder = admin.Username,
                };


                context.Users.Add(admin);
                context.Accounts.Add(admin.Account);
                await context.SaveChangesAsync();
                return;
            }

            var changed = false;

            if (admin.Role != UserRole.Admin)
            {
                admin.Role = UserRole.Admin;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(admin.PasswordHash))
            {
                admin.PasswordHash = BC.HashPassword(adminPassword);
                changed = true;
            }

            if (admin.Account == null)
            {
                admin.Account = new Account
                {
                    UserId = admin.Id,
                    BankName = "Default Bank",
                    AccountNumber = $"ACC-{admin.Id:N}".Substring(0, 16),
                    AccountHolder = admin.Username,
                };

                context.Accounts.Add(admin.Account);
                changed = true;
            }

            if (changed)
                await context.SaveChangesAsync();
        }

    }   

}
