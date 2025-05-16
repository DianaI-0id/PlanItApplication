using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class MyBonusesUserControl : UserControl, INotifyPropertyChanged
{

    private bool _isCardsExists;
    public bool IsCardsExists
    {
        get => _isCardsExists;
        set
        {
            if (_isCardsExists != value)
            {
                _isCardsExists = value;
                OnPropertyChanged(nameof(IsCardsExists));
            }
        }
    }

    public ObservableCollection<string> CardSectionsCollection { get; set; } = new ObservableCollection<string>();
    public ObservableCollection<UserGiftCard> GiftCardsCollection { get; set; } = new ObservableCollection<UserGiftCard>();

    public MyBonusesUserControl()
    {
        InitializeComponent();
        LoadGiftCards();
        LoadCardSections();

        DataContext = this;
    }

    private void LoadGiftCards()
    {
        var giftcard = BonusProgramService.LoadUserGiftCardsByUser(AuthorizedUser.User);

        GiftCardsCollection = new ObservableCollection<UserGiftCard>(giftcard);
        OnPropertyChanged(nameof(GiftCardsCollection));

        IsCardsExists = GiftCardsCollection.Any();
        OnPropertyChanged(nameof(IsCardsExists));
    }

    private void LoadCardSections()
    {
        var sections = BonusProgramService.LoadCardSectionsByUser(AuthorizedUser.User);

        CardSectionsCollection.Clear();

        CardSectionsCollection.Add("Все типы");
        foreach (var card in sections)
        {
            CardSectionsCollection.Add(card);
        }
        OnPropertyChanged(nameof(GiftCardsCollection));
    }

    private void SelectCardSection_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var filtered = BonusProgramService.LoadUserGiftCardsByUser(AuthorizedUser.User);

        if (sender is ComboBox combo && combo.SelectedItem is string item)
        {
            if (item != "Все типы")
            {
                filtered = filtered.Where(u => u.GiftCard?.CardSection == item).ToList();
            }
            else
            {
                filtered = filtered.ToList();
            }

            GiftCardsCollection.Clear();
            foreach (var card in filtered)
            {
                GiftCardsCollection.Add(card);
            }
        }
    }

    private async void MarkAsUsed_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is UserGiftCard giftCard)
        {
            if (giftCard != null)
            {
                var messageBox = new MessageBox(message: "Вы не сможете отменить это действие, вы уверены?", true);
                var result = await messageBox.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);

                if (result)
                {
                    giftCard.IsUsed = true;
                    giftCard.UsedAt = DateTime.Now;

                    await BonusProgramService.UpdateGiftCard(giftCard);

                    // Удаляем карту из коллекции
                    GiftCardsCollection.Remove(giftCard);
                    OnPropertyChanged(nameof(GiftCardsCollection));

                    // Добавляем обновление IsCardsExists
                    IsCardsExists = GiftCardsCollection.Any();
                    OnPropertyChanged(nameof(IsCardsExists)); // <- Ключевое исправление
                }
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}