using Diploma_Ishchenko.DatabaseData.Context;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace Diploma_Ishchenko
{
    public static class EmailSender
    {
        private const string SenderEmail = "dlya.urokov9kl@gmail.com";
        private const string SenderPassword = "bhli ztzh islj rouk";

        public static async Task SendNewPasswordAsync(string recipientEmail)
        {
            using (var context = new PlanItContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == recipientEmail);

                string newPassword = GenerateTemporaryPassword();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                user.PasswordHash = hashedPassword;
                await context.SaveChangesAsync();

                await SendEmailAsync(recipientEmail, newPassword);
            }     
        }

        private static string GenerateTemporaryPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static async Task SendEmailAsync(string recipientEmail, string newPassword)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Сообщество PlanIt", SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Восстановление пароля";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = @"
                    <div style=""background-color: #FFFFFF; padding: 20px; border: 1px solid #DDDDDD;"">
                        <h1 style=""color: #007bff;"">PlanIt</h1>
                        <div style=""background-color: #007bff; padding: 10px; color: #FFFFFF; border-radius: 5px;"">
                            Ваш новый пароль: <strong>" + newPassword + @"</strong>
                        </div>
                        <p>Новый пароль успешно сохранен.</p>
                    </div>
                ";

            message.Body = bodyBuilder.ToMessageBody();


            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(SenderEmail, SenderPassword);
                    await client.SendAsync(message);
                    Console.WriteLine("Письмо успешно отправлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public static async Task SendUserGiftCardAsync(string userEmail)
        {
            using var context = new PlanItContext();

            // Получаем пользователя с бонусной картой
            var userGiftCard = context.UserGiftCards
                .Where(ugc => ugc.User != null && ugc.User.Email == userEmail && ugc.IsUsed != true)
                .OrderByDescending(ugc => ugc.CreatedAt)
                .Select(ugc => new
                {
                    ugc.CardNumber,
                    ugc.PIN,
                    ugc.ExpirationDate,
                    GiftCard = ugc.GiftCard
                })
                .FirstOrDefault();

            if (userGiftCard == null)
            {
                Console.WriteLine("Пользователь не найден или у него нет активной бонусной карты.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Сообщество PlanIt", SenderEmail));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = $"Ваша бонусная карта {userGiftCard.GiftCard.CardType}";

            var bodyBuilder = new BodyBuilder();

            // Формируем HTML тело письма с версткой, похожей на ваш пример
            bodyBuilder.HtmlBody = $@"
            <div style=""background-color: #222; color: #fff; padding: 20px; border-radius: 10px; font-family: Arial, sans-serif;"">
                <div style=""background-color: #ffe066; color: #222; padding: 20px; border-radius: 10px 10px 0 0;"">
                    <h2 style=""margin: 0;"">{userGiftCard.GiftCard.CardType}</h2>
                    <p style=""margin: 0; font-weight: bold;"">Подарочная карта</p>
                    <p style=""margin: 0;"">{userGiftCard.GiftCard.CardSection}</p>
                    <p style=""margin: 0; font-size: 1.2em;""><b>Баланс: {userGiftCard.GiftCard.Balance} ₽</b></p>
                </div>
                <div style=""margin-top: 20px; background: #333; padding: 15px; border-radius: 0 0 10px 10px;"">
                    <h3>Данные для активации карты</h3>
                    <p><b>Номер карты:</b> {userGiftCard.CardNumber}</p>
                    <p><b>PIN:</b> {userGiftCard.PIN}</p>
                    <p><b>Действует до:</b> {(userGiftCard.ExpirationDate.HasValue ? userGiftCard.ExpirationDate.Value.ToString("dd.MM.yyyy") : "Без срока")}</p>
                    <p style=""font-size: 0.9em;"">Введите эти данные в соответствующем интернет-магазине до истечения срока действия карты.</p>
                </div>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(SenderEmail, SenderPassword);
                await client.SendAsync(message);
                Console.WriteLine("Письмо с бонусной картой успешно отправлено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
