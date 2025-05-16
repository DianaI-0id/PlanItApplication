using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using System;

namespace Diploma_Ishchenko;

public partial class BanUserWindow : Window
{
    private User _user; //пользователь которого будем банить
    private string _selectionDuration = string.Empty;
    private DateTime _endBanDate;

    public BanUserWindow()
    {
        InitializeComponent();
    }

    public BanUserWindow(User user)
    {
        InitializeComponent();
        _user = user;
        DataContext = this;
    }

    private void SelectDuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
        {
            _selectionDuration = item.Content.ToString();

            switch (_selectionDuration)
            {
                case "3 дня":
                    _endBanDate = DateTime.Now.AddDays(3);
                    break;
                case "7 дней":
                    _endBanDate = DateTime.Now.AddDays(7);
                    break;
                case "1 мес":
                    _endBanDate = DateTime.Now.AddMonths(1);
                    break;
                case "полгода":
                    _endBanDate = DateTime.Now.AddMonths(6);
                    break;
                case "год":
                    _endBanDate = DateTime.Now.AddYears(1);
                    break;
                case "навсегда":
                    _endBanDate = DateTime.Now.AddYears(100);
                    break;
            }
        }
    }

    private async void ApplyBanning_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ReasonTextBox.Text) && !string.IsNullOrEmpty(_selectionDuration))
        {
            await BannedUsersService.BanUser(_user, ReasonTextBox.Text, _endBanDate);

            var msBox = new MessageBox("Пользователь успешно заблокирован!", false);
            await msBox.ShowDialog(this);

            this.Close(true);
        }
    }
}