using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class GiftCard
{
    public int Id { get; set; }

    public string CardType { get; set; } = null!;

    public string CardSection { get; set; } = null!; //раздел, например Техника

    public decimal Balance { get; set; }

    public int Quantity { get; set; }

    public bool IsNotExistsVisible
    {
        get
        {
            return Quantity <= 0;
        }
    }

    //нот маппед переменная для вывода опред цветом карточку
    [NotMapped]
    public string BorderCardColor { get; set; }

    [NotMapped] // Стоимость карты в бонусах (расчёт по таблице)
    //при стоимости подписки в 490р и максимальном кол-ве баллов 5 в день пользователь в мес может заработать 150 баллов
    public decimal BonusCost
    {
        get
        {
            return Balance switch
            {
                199 => 70,     // 2.84 ₽/балл
                300 => 105,    // 2.86 ₽/балл
                500 => 175,    // 2.86 ₽/балл
                520 => 182,    // 2.86 ₽/балл
                999 => 345,     // 2.90 ₽/балл
                1000 => 345,    // 2.90 ₽/балл
                1790 => 620,    // 2.89 ₽/балл
                2000 => 690,    // 2.90 ₽/балл
                3000 => 1035,   // 2.90 ₽/балл
                5000 => 1725,   // 2.90 ₽/балл
                10000 => 3450,  // 2.90 ₽/балл
                _ => (int)Math.Ceiling(Balance / 2.9m) // Дефолт: ~2.90 ₽/балл
            };
        }
    }

   

    public virtual ICollection<UserGiftCard> UserGiftCards { get; set; } = new List<UserGiftCard>();
}
