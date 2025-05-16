using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.PostgresServices
{
    public static class BannedUsersService
    {
        public static bool FindBannedUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            using (var context = new PlanItContext())
            {
                try
                {
                    CheckBannedUsers();

                    var userId = user.Id;
                    return context.BannedUsers.Any(b => b.UserId == userId);
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    Console.WriteLine($"Error finding banned user: {ex.Message}");
                    throw;
                }
            }
        }

        public static BannedUser GetBannedUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            using (var context = new PlanItContext())
            {
                try
                {
                    var userId = user.Id;
                    return context.BannedUsers.FirstOrDefault(b => b.UserId == userId);
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    Console.WriteLine($"Error getting banned user: {ex.Message}");
                    throw;
                }
            }
        }

        public static void CheckBannedUsers()
        {
            using (var context = new PlanItContext())
            {
                try
                {
                    var currentDate = DateTime.Now;
                    var expiredBans = context.BannedUsers
                        .Where(b => b.BanEnddate < currentDate)
                        .ToList();

                    if (expiredBans.Any())
                    {
                        context.BannedUsers.RemoveRange(expiredBans);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    Console.WriteLine($"Error checking banned users: {ex.Message}");
                    throw;
                }
            }
        }

        public static async Task BanUser(User user, string reason, DateTime endDate)
        {
            using (var context = new PlanItContext())
            {
                var bannedUser = new BannedUser
                {
                    Id = context.BannedUsers.Any() ? context.BannedUsers.Max(u => u.Id) + 1 : 1,
                    UserId = user.Id,
                    Reason = reason,
                    BanDate = DateTime.Now,
                    BanEnddate = endDate
                };

                await context.BannedUsers.AddAsync(bannedUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
