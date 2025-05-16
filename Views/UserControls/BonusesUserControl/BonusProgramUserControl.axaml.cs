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

public partial class BonusProgramUserControl : UserControl, INotifyPropertyChanged
{
    public ObservableCollection<string> CardSectionsCollection { get; set; } = new ObservableCollection<string>();
    public ObservableCollection<GiftCard> GiftCardsCollection { get; set; } = new ObservableCollection<GiftCard>();

    public bool IsUserHasSubscription => AuthorizedUser.User.HasActiveSubscription == true;
    public int UserBonusesCount { get; set;
    }
    public BonusProgramUserControl()
    {
        InitializeComponent();
        LoadGiftCards();
        LoadCardSections();
        ShowUserBonusesCount();

        DataContext = this;
    }

    private void ShowUserBonusesCount()
    {
        var user = UserService.GetUserByEmail(AuthorizedUser.User.Email);

        //������� ���-�� ������ ������������
        UserBonusesCount = user.Point.Amount ?? 0;
        OnPropertyChanged(nameof(UserBonusesCount));
    }

    private void LoadGiftCards()
    {
        var giftcard = BonusProgramService.LoadGiftCard();

        GiftCardsCollection = new ObservableCollection<GiftCard>(giftcard);
        OnPropertyChanged(nameof(GiftCardsCollection));
    }

    private void LoadCardSections()
    {
        var sections = BonusProgramService.LoadCardSections();

        CardSectionsCollection.Clear();

        CardSectionsCollection.Add("��� ����");
        foreach (var card in sections)
        {
            CardSectionsCollection.Add(card);
        }
        OnPropertyChanged(nameof(GiftCardsCollection));
    }

    private void SelectCardSection_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var filtered = BonusProgramService.LoadGiftCard();

        if (sender is ComboBox combo && combo.SelectedItem is string item)
        {
            if (item != "��� ����")
            {
                filtered = filtered.Where(u => u.CardSection == item).ToList();
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

    //����� ������ �� �������� �����
    private async void ExchangeBonusesForCard_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is GiftCard card)
        {
            var currentUser = UserService.GetUserByEmail(AuthorizedUser.User.Email);
            if (currentUser.Point.Amount < card.BonusCost)
            {
                var contentContainer = this.GetVisualRoot() as ContentContainer;
                await new MessageBox(message: "������������ ������ ��� ������", false).ShowDialog(contentContainer);
                return;
            }

            if (card.Quantity <= 0)
            {
                var contentContainer = this.GetVisualRoot() as ContentContainer;
                await new MessageBox(message: "������ ����� ������ ��� � �������", false).ShowDialog(contentContainer);
                return;
            }

            var giftCard = new UserGiftCard
            {
                UserId = AuthorizedUser.User.Id,
                GiftCardId = card.Id,
                CardNumber = BonusProgramService.GenerateCardNumber(),
                PIN = BonusProgramService.GenerateCardPin(),
                ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                CreatedAt = DateTime.Now,
                IsUsed = false //�� ��������� false
            };

            await BonusProgramService.AddUserGiftCard(giftCard);

            this.IsEnabled = false;

            await EmailSender.SendUserGiftCardAsync(AuthorizedUser.User.Email);
            this.IsEnabled = true;



            //������ ��� ����� ��������� ��������� ��� ������� ���������� ������ ����� 
            card.Quantity--;
            await BonusProgramService.UpdateGiftCard(card);

            //��������� ������ � ������ ������������
            currentUser = UserService.GetUserByEmail(AuthorizedUser.User.Email);
            currentUser.Point.Amount -= Convert.ToInt32(card.BonusCost);

            await UserService.UpdateUser(currentUser);

            //��������� ��������� �������� ��������������� �����������
            AuthorizedUser.User = currentUser;


            //������������� ����� ��� ������ �� � �������� �������
            LoadGiftCards();
            LoadCardSections();
            ShowUserBonusesCount();

            //����� ����� ������� ��������� �� �������� 
            var container = this.GetVisualRoot() as ContentContainer;
            await new MessageBox(message: "����� ������� ��������� �� ��� ������� � ���������� �� �����! ���� �� �� �� �����, ��������� ����� ����", false).ShowDialog(container);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}