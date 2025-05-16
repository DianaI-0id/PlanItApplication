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
    private User _user; //������������ �������� ����� ������
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
                case "3 ���":
                    _endBanDate = DateTime.Now.AddDays(3);
                    break;
                case "7 ����":
                    _endBanDate = DateTime.Now.AddDays(7);
                    break;
                case "1 ���":
                    _endBanDate = DateTime.Now.AddMonths(1);
                    break;
                case "�������":
                    _endBanDate = DateTime.Now.AddMonths(6);
                    break;
                case "���":
                    _endBanDate = DateTime.Now.AddYears(1);
                    break;
                case "��������":
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

            var msBox = new MessageBox("������������ ������� ������������!", false);
            await msBox.ShowDialog(this);

            this.Close(true);
        }
    }
}