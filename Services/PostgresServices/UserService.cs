using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Google.Apis.Oauth2.v2.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.PostgresServices
{
    public static class UserService
    {

        public static List<User> LoadUsers()
        {
            using (var context = new PlanItContext())
            {
                var users = context.Users.Include(p => p.PointsHistories);
                return users.ToList();
            }
        }

        public static async Task AddUser(User user)
        {
            using (var context = new PlanItContext())
            {
                user.Id = context.Users.Any() ? context.Users.Max(u => u.Id) + 1 : 1;
                user.Biography = "Информация отсутствует";
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
        }

        public static async Task UpdateUser(User user)
        {
            using (var context = new PlanItContext())
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }

        public static User GetUserByEmail(string email)
        {
            using (var context = new PlanItContext())
            {
                var user = context.Users
                    .Include(p => p.PointsHistories)
                    .Include(g => g.UserGiftCards)
                        .ThenInclude(g => g.GiftCard)
                    .Include(b => b.Point)
                    .FirstOrDefault(x => x.Email.ToLower() == email);
                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public static int GetSubscriptionsCount(int userId)
        {
            using var db = new PlanItContext();
            return db.Subscriptions.Count(s => s.SubscriberId == userId);
        }

        public static int GetSubscribersCount(int userId)
        {
            using var db = new PlanItContext();
            return db.Subscriptions.Count(s => s.SubscribedToId == userId);
        }

        //пригодится для авторизации через логин или email
        public static User GetUserByNickname(string nickname)
        {
            using (var context = new PlanItContext())
            {
                var user = context.Users.FirstOrDefault(x => x.Nickname == nickname);
                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public static User GetUserById(int Id)
        {
            using (var context = new PlanItContext())
            {
                var user = context.Users.FirstOrDefault(x => x.Id == Id);
                if (user != null)
                {
                    return user;
                }
            }

            return null;
        }

        public static async Task AddBonusWalletToUser(User user)
        {
            using (var context = new PlanItContext())
            {
                var bonusWallet = new Point
                {
                    UserId = user.Id,
                    Amount = 0,
                    LastUpdated = DateTime.Now
                };

                context.Points.Add(bonusWallet);
                await context.SaveChangesAsync();
            }
        }

        // Обновление аватарки пользователя
        public static async Task UpdateUserAvatar(int userId, string relativeImagePath)
        {
            using (var context = new PlanItContext())
            {
                var user = await context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Userphoto = relativeImagePath;
                    await context.SaveChangesAsync();
                }
            }
        }

        // Получение относительного пути аватарки
        public static string GetUserAvatarPath(int userId)
        {
            using (var context = new PlanItContext())
            {
                return context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Userphoto)
                    .FirstOrDefault();
            }
        }

        // Обновление пользователя с новой фотографией
        public static async Task UpdateUserWithAvatar(User user, string relativeImagePath)
        {
            using (var context = new PlanItContext())
            {
                user.Userphoto = relativeImagePath;
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }

        //обновляем данные никнейма и пароля пользователя
        public static async void SetUserNicknameAndPasswordAsync(User user, string nickname, string password)
        {
            using (var context = new PlanItContext())
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                user.Nickname = nickname;
                user.PasswordHash = hashedPassword;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }

        public static async Task<User> AddUserByGoogle(Userinfo userInfo)
        {
            using (var context = new PlanItContext())
            {
                var user = new User
                {
                    Username = userInfo.GivenName + " " + userInfo.FamilyName,
                    Email = userInfo.Email,
                    CreatedAt = DateTime.Now,
                    IsAdmin = false,
                    GoogleId = userInfo.Id,
                    Roleid = 1,
                    Nickname = $"{userInfo.Email}{userInfo.GivenName}{userInfo.FamilyName}", //по умолчанию задаем пока такой никнейм чтобы избежать повтора, в следующем окне его все равно переопределяем
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), //ставим по умолчанию чтобы избежать эксепшена
                    Userphoto = "Avatars/default/no-profile-picture.png" //по умолчанию заглушка
                };

                //context.Users.Add(user);
                //await context.SaveChangesAsync();
                ////помимо юзера добавляем ему по умолчанию базовую цветовую тему

                //var userSettings = new Usersetting
                //{
                //    Id = context.Usersettings.Any() ? context.Usersettings.Max(u => u.Id) + 1 : 1,
                //    Userid = user.Id,
                //    Colorthemeid = 1 //по умолчанию осталяем Blue theme
                //};
                //context.Usersettings.Add(userSettings);

                //await context.SaveChangesAsync();

                return user;
            }
            
        }
    }
}
