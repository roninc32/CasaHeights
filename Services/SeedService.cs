using CasaHeights.Data;
using CasaHeights.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CasaHeights.Services
{
    public class SeedService
    {
        private readonly ILogger<SeedService> _logger;

        public SeedService(ILogger<SeedService> logger)
        {
            _logger = logger;
        }

        public async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            try
            {
                var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure database is created and migrations are applied
                await dbContext.Database.MigrateAsync();

                // Seed roles if they don't exist
                string[] roleNames = { "Admin", "Staff", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        _logger.LogInformation($"Created role: {roleName}");
                    }
                }

                // Seed admin user if it doesn't exist
                if (!dbContext.Users.Any(u => u.UserName == "admin@casaheights.com"))
                {
                    var adminUser = new Users
                    {
                        UserName = "admin@casaheights.com",
                        Email = "admin@casaheights.com",
                        EmailConfirmed = true,
                        FullName = "Admin User",
                        HouseNumber = "Admin", // Ensure HouseNumber is set
                        PhoneNumber = "1234567890",
                        PhoneNumberConfirmed = true,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        _logger.LogInformation("Admin user created successfully");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"Error creating admin user: {error.Description}");
                        }
                    }
                }

                // Seed staff user if it doesn't exist
                if (!dbContext.Users.Any(u => u.UserName == "staff@casaheights.com"))
                {
                    var staffUser = new Users
                    {
                        UserName = "staff@casaheights.com",
                        Email = "staff@casaheights.com",
                        EmailConfirmed = true,
                        FullName = "Staff Member",
                        HouseNumber = "Staff", // Ensure HouseNumber is set
                        PhoneNumber = "0987654321",
                        PhoneNumberConfirmed = true,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(staffUser, "Staff@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(staffUser, "Staff");
                        _logger.LogInformation("Staff user created successfully");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"Error creating staff user: {error.Description}");
                        }
                    }
                }

                // Seed sample user if needed
                if (!dbContext.Users.Any(u => u.UserName == "user@example.com"))
                {
                    var regularUser = new Users
                    {
                        UserName = "user@example.com",
                        Email = "user@example.com",
                        EmailConfirmed = true,
                        FullName = "Sample User",
                        HouseNumber = "101", // Ensure HouseNumber is set
                        PhoneNumber = "5551234567",
                        PhoneNumberConfirmed = true,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(regularUser, "User@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(regularUser, "User");
                        _logger.LogInformation("Sample user created successfully");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"Error creating sample user: {error.Description}");
                        }
                    }
                }

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}