using System;
using System.Collections.Generic;
using System.Linq;
using Backend.API.Data;
using Backend.API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.API.Tests.Integration.TestConfiguration;

public class ApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContextPool<ApplicationDbContext>(options => { options.UseInMemoryDatabase("TestDB"); });

            // create a new DB context
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            ConfigureRoles(db);
            ConfigureUsers(db);
            ConfigureLabs(db);

            db.Database.EnsureCreated();
        });
    }

    private static void ConfigureRoles(ApplicationDbContext db)
    {
        db.Roles.AddRange([
            new ApplicationRole("user")
            {
                Id = "03B11179-8A33-4D3B-8092-463249F755A5",
                NormalizedName = "USER",
                ConcurrencyStamp = "ABCD1254-FEE2-42D0-A96E-E2018C9161BF"
            },
            new ApplicationRole("admin")
            {
                Id = "CF0B61E1-3BB2-40D6-8E17-60CF475CE884",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "D5FC26A2-75FD-45F8-AB88-DF4D337C5910"
            }
        ]);

        db.RoleClaims.AddRange([
            new IdentityRoleClaim<string>
            {
                RoleId = "03B11179-8A33-4D3B-8092-463249F755A5",
                Id = 1,
                ClaimType = "Permission",
                ClaimValue = "Administrative.ViewUsers"
            },
            new IdentityRoleClaim<string>
            {
                RoleId = "CF0B61E1-3BB2-40D6-8E17-60CF475CE884",
                Id = 2,
                ClaimType = "Permission",
                ClaimValue = "Administrative.ViewUsers"
            },
            new IdentityRoleClaim<string>
            {
                RoleId = "CF0B61E1-3BB2-40D6-8E17-60CF475CE884",
                Id = 3,
                ClaimType = "Permission",
                ClaimValue = "Administrative.ManageUsers"
            }
        ]);

        db.SaveChanges();
    }

    private static void ConfigureUsers(ApplicationDbContext db)
    {
        db.Users.AddRange([
            new ApplicationUser
            {
                Email = "joedoe@gmail.com",
                ConcurrencyStamp = "2fe7f8d5-d321-4c77-884b-73ea438b1511",
                LockoutEnabled = true,
                Id = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb",
                NormalizedEmail = "JOEDOE@GMAIL.COM",
                PhoneNumber = "0963233542",
                EmailConfirmed = true,
                Name = "Joe",
                Family = "Doe",
                RegisterDate = DateTime.Now,
                SecurityStamp = "LJNTPIYBD4KN2CFESBRMRL2YDQOXANQ4",
                UserName = "joedoe@gmail.com",
                NormalizedUserName = "JOEDOE@GMAIL.COM",
                PhoneNumberConfirmed = true,
                RefreshTokenExpireTime = DateTime.MinValue,
                PasswordHash =
                    "AQAAAAIAAYagAAAAEK1W3FMebsaQ5p6sqwXybnO6AdMcllqC99NBccKaS99FJZji0MmRjLfY4vMAR/ldRA=="
            },
            new ApplicationUser
            {
                Email = "jilldoe@gmail.com",
                ConcurrencyStamp = "6b263a8b-120f-4f48-a6bd-ad3a9c4c913d",
                LockoutEnabled = true,
                Id = "f0dccee8-a3e1-45f8-9bb7-f7e7decebd09",
                NormalizedEmail = "JILLDOE@GMAIL.COM",
                PhoneNumber = "0963233542",
                EmailConfirmed = true,
                Name = "Jill",
                Family = "Doe",
                RegisterDate = DateTime.Now,
                SecurityStamp = "OHACRUB556PUCIJOKNPX6QMTHA5G77DG",
                UserName = "jilldoe@gmail.com",
                NormalizedUserName = "JILLDOE@GMAIL.COM",
                PhoneNumberConfirmed = true,
                RefreshTokenExpireTime = DateTime.MinValue,
                PasswordHash =
                    "AQAAAAIAAYagAAAAEK1W3FMebsaQ5p6sqwXybnO6AdMcllqC99NBccKaS99FJZji0MmRjLfY4vMAR/ldRA=="
            },
            new ApplicationUser
            {
                Email = "admin@gmail.com",
                ConcurrencyStamp = "6b263a8b-120f-4f48-a6bd-ad3a9c4c913d",
                LockoutEnabled = true,
                Id = "0c692e0f-f0e6-4f19-b138-f93fdb7b094d",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                PhoneNumber = "0963233542",
                EmailConfirmed = true,
                Name = "Admin",
                Family = "Adminify",
                RegisterDate = DateTime.Now,
                SecurityStamp = "OHACRUB556PUCIJOKNPX6QMTHA5G77DG",
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                PhoneNumberConfirmed = true,
                RefreshTokenExpireTime = DateTime.MinValue,
                PasswordHash =
                    "AQAAAAIAAYagAAAAEK1W3FMebsaQ5p6sqwXybnO6AdMcllqC99NBccKaS99FJZji0MmRjLfY4vMAR/ldRA=="
            }
        ]);

        db.UserRoles.AddRange([
            new ApplicationUserRole
            {
                UserId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb",
                RoleId = "03B11179-8A33-4D3B-8092-463249F755A5"
            },
            new ApplicationUserRole
            {
                UserId = "f0dccee8-a3e1-45f8-9bb7-f7e7decebd09",
                RoleId = "03B11179-8A33-4D3B-8092-463249F755A5"
            },
            new ApplicationUserRole
            {
                UserId = "0c692e0f-f0e6-4f19-b138-f93fdb7b094d",
                RoleId = "CF0B61E1-3BB2-40D6-8E17-60CF475CE884"
            }
        ]);

        db.SaveChanges();
    }

    private static void ConfigureLabs(ApplicationDbContext db)
    {
        db.Labs.AddRange([
            new Lab
            {
                Id = "0375aa61-00b4-46f6-a704-a1cadce5543a",
                UserId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb",
                Name = "Lab 1",
                SubmissionDate = default
            },
            new Lab
            {
                Id = "1b2fb6ac-0237-4520-9076-6fc5fc0874b6",
                UserId = "6b263a8b-120f-4f48-a6bd-ad3a9c4c913d",
                Name = "Lab 2",
                SubmissionDate = default
            }
        ]);

        db.LabFiles.AddRange([
            new LabFile
            {
                Id = "7f48acf1-9474-4863-b966-543dc92b3e8d",
                LabId = "0375aa61-00b4-46f6-a704-a1cadce5543a",
                Name = "Program.cs",
                FileContent =
                    """using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Text;\nusing System.Threading.Tasks;\n\nnamespace ConsoleApp1\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine("Hello, world!");\n            Console.ReadLine();\n        }\n    }\n}"""
            },
            new LabFile
            {
                Id = "8d05d09a-ce99-4a8c-b174-9b65a4850e0b",
                LabId = "1b2fb6ac-0237-4520-9076-6fc5fc0874b6",
                Name = "Program.cs",
                FileContent = """Console.WriteLine("Hello, World!");"""
            }
        ]);

        db.SaveChanges();
    }
}