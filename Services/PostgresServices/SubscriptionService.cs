using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.PostgresServices
{
    public static class SubscriptionService
    {
        // Метод для загрузки подписок (кого подписан пользователь)
        public static async Task<List<User>> GetSubscriptionsAsync(int userId)
        {
            using (var context = new PlanItContext())
            {
                return await context.Subscriptions
                    .Include(s => s.SubscribedTo)
                    .Where(s => s.SubscriberId == userId && s.SubscribedTo != null)
                    .Select(s => s.SubscribedTo!)
                    .ToListAsync();
            }
        }

        // Метод для загрузки подписчиков (кто подписан на пользователя)
        public static async Task<List<User>> GetSubscribersAsync(int userId)
        {
            using (var context = new PlanItContext())
            {
                return await context.Subscriptions
                    .Include(s => s.Subscriber)
                    .Where(s => s.SubscribedToId == userId && s.Subscriber != null)
                    .Select(s => s.Subscriber!)
                    .ToListAsync();
            }
        }

        public static async Task AddSubscriptionData(User user)
        {
            using (var context = new PlanItContext())
            {
                var subscriptionHistory = new SubscriptionHistory
                {
                    Id = context.SubscriptionHistories.Any() ? context.SubscriptionHistories.Max(u => u.Id) + 1 : 1,
                    UserId = user.Id,
                    SubscriptionStartDate = (DateTime)user.SubscriptionStartDate,
                    SubscriptionEndDate = (DateTime)user.SubscriptionEndDate,
                    PaymentAmount = 490,
                    CreatedAt = DateTime.Now
                };

                await context.SubscriptionHistories.AddAsync(subscriptionHistory);
                await context.SaveChangesAsync();
            }
        }

        public static bool IsSubscribed(int currentUserId, int targetUserId)
        {
            // Логика проверки подписки (например, запрос в БД)
            // Возвращает true, если подписка есть, иначе false
            using (var context = new PlanItContext())
            {
                return context.Subscriptions.Any(s => s.SubscriberId == currentUserId && s.SubscribedToId == targetUserId);
            }
        }

        public static async Task DropSubscription(int currentUserId, int targetUserId)
        {
            using (var context = new PlanItContext())
            {
                var field = context.Subscriptions.FirstOrDefault(s => s.SubscriberId == currentUserId && s.SubscribedToId == targetUserId);
                context.Subscriptions.Remove(field);

                await context.SaveChangesAsync();
            }
        }

        public static async Task SubscribeToUser(User currentUser, User targetUser)
        {
            using (var context = new PlanItContext())
            {
                var subscribe = new Subscription
                {
                    Id = context.Subscriptions.Any() ? context.Subscriptions.Max(u => u.Id) + 1 : 1,
                    SubscriberId = currentUser.Id, 
                    SubscribedToId = targetUser.Id,
                    CreatedAt = DateTime.Now
                };

                context.Subscriptions.Add(subscribe);
                await context.SaveChangesAsync();
            }
        }

        public static async Task DropUserSubscribe(User currentUser, User targetUser)
        {
            using (var context = new PlanItContext())
            {
                var subscription = context.Subscriptions.FirstOrDefault(s => s.SubscriberId == currentUser.Id && s.SubscribedToId == targetUser.Id);
                if (subscription != null)
                {
                    context.Subscriptions.Remove(subscription);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
