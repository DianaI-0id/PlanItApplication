using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.DataCheckers;
using Diploma_Ishchenko.Services.PostgresServices;
using System;

namespace Diploma_Ishchenko;

public partial class SetNicknameAndPassword : Window
{
    private readonly User _user;

    public SetNicknameAndPassword(User user)
    {
        InitializeComponent();
        _user = user;
    }

    private async void ConfirmData_ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidateInput())
                return;

            // ���������� ����������
            _user.Nickname = NicknameTextBox.Text;
            _user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordTextBox.Text);
            await UserService.UpdateUser(_user);

            Close(true);
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"������: {ex.Message}");
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrEmpty(NicknameTextBox.Text))
        {
            ShowErrorMessage("������� �������");
            return false;
        }

        if (UserService.GetUserByNickname(NicknameTextBox.Text) != null)
        {
            ShowErrorMessage("������� ��� �����");
            return false;
        }

        if (PasswordTextBox.Text != RepeatPasswordTextBox.Text)
        {
            ShowErrorMessage("������ �� ���������");
            return false;
        }

        if (string.IsNullOrEmpty(PasswordTextBox.Text) || string.IsNullOrEmpty(RepeatPasswordTextBox.Text))
        {
            ShowErrorMessage("������ �� ����� ���� �������");
            return false;
        }

        if (!DataCheckerClass.IsValidPassword(PasswordTextBox.Text))
        {
            ShowErrorMessage("������ ������ ��������� �����, ����� �������� � ������� ��������");
            return false;
        }

        return true;
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage.Text = message;
        ErrorMessage.IsVisible = true;
    }
}

