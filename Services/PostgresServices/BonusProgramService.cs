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
    public static class BonusProgramService
    {
        public static List<GiftCard> LoadGiftCard()
        {
            using (var context = new PlanItContext())
            {
                var cards = context.GiftCards
                    .Include(g => g.UserGiftCards)
                    .OrderBy(u => u.Id);

                UpdateBorderColorByCard(cards.ToList());
                return cards.ToList();
            }
        }

        public static async Task UpdateGiftCard(UserGiftCard card)
        {
            using (var context = new PlanItContext())
            {
                context.UserGiftCards.Update(card);
                await context.SaveChangesAsync();
            }
        }

        public static List<UserGiftCard> LoadUserGiftCardsByUser(User user)
        {
            using (var context = new PlanItContext())
            {
                var userGiftCards = context.UserGiftCards
                    .Include(ug => ug.GiftCard)
                    .Where(ug => ug.UserId == user.Id
                                 && ug.IsUsed == false
                                 && ug.ExpirationDate > DateOnly.FromDateTime(DateTime.Now))
                    .ToList();

                // Обновляем цвет у связанных GiftCard
                var giftCards = userGiftCards
                    .Where(ugc => ugc.GiftCard != null)
                    .Select(ugc => ugc.GiftCard!)
                    .ToList();

                UpdateBorderColorByCard(giftCards);

                return userGiftCards;
            }
        }


        public static List<string> LoadCardSectionsByUser(User user)
        {
            using (var context = new PlanItContext())
            {
                return context.UserGiftCards
                    .Include(ug => ug.GiftCard)
                    .Where(ug => ug.UserId == user.Id
                        && ug.IsUsed == false
                        && ug.ExpirationDate > DateOnly.FromDateTime(DateTime.Now))
                    .Select(ug => ug.GiftCard.CardSection)
                    .Distinct()
                    .ToList();
            }
        }


        public static UserGiftCard GetUserGiftCard(GiftCard giftCard)
        {
            using (var context = new PlanItContext())
            {
                var userGiftCard = context.UserGiftCards.FirstOrDefault(g => g.GiftCardId == giftCard.Id);

                return userGiftCard;
            }
        }

        private static void UpdateBorderColorByCard(List<GiftCard> cards)
        {
            foreach (var card in cards)
            {
                switch (card.CardSection)
                {
                    case "Электроника":
                        card.BorderCardColor = "#5474e9"; //мутно-голубой
                        break;
                    case "Книги":
                        card.BorderCardColor = "#ede169"; //мутно-желтый
                        break;
                    case "Косметика":
                        card.BorderCardColor = "#ed68c5"; //мутно-розовый
                        break;
                    case "Подписки":
                        card.BorderCardColor = "#eb4460"; //мутно-красный
                        break;
                }
            }
        }

        public static List<string> LoadCardSections()
        {
            using (var context = new PlanItContext())
            {
                var sections = context.GiftCards.Select(c => c.CardSection).Distinct();
                return sections.ToList();
            }
        }

        public static string GenerateCardNumber()
        {
            using (var context = new PlanItContext())
            {
                Random rnd = new Random();
                var result = string.Empty;
                do
                {
                    result = rnd.NextInt64(1000000000000, 9999999999999).ToString();
                }
                while (
                    context.UserGiftCards.Any(u => u.CardNumber == result)
                );

                return result;
            }
        }

        public static string GenerateCardPin()
        {
            using (var context = new PlanItContext())
            {
                Random rnd = new Random();
                var result = string.Empty;
                do
                {
                    result = rnd.NextInt64(100000000, 999999999).ToString();
                }
                while (
                    context.UserGiftCards.Any(u => u.PIN == result)
                );

                return result;
            }
        }

        public static async Task AddUserGiftCard(UserGiftCard card)
        {
            using (var context = new PlanItContext())
            {
                card.Id = context.UserGiftCards.Any() ? context.UserGiftCards.Max(u => u.Id) + 1 : 1;

                await context.UserGiftCards.AddAsync(card);
                await context.SaveChangesAsync();
            }
        }

        public static async Task UpdateGiftCard(GiftCard card)
        {
            using (var context = new PlanItContext())
            {
                context.GiftCards.Update(card);
                await context.SaveChangesAsync();
            }
        }
    }
}
