using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.Services.PostgresServices;

namespace Diploma_Ishchenko;

public partial class ForgetPassword : Window
{
    public ForgetPassword()
    {
        InitializeComponent();
    }

    private async void SendEmail_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(UseremailTextBox.Text))
        {
            string emailText = UseremailTextBox.Text;

            // ���������� ��������� ��� �������� email
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

            if (emailRegex.IsMatch(emailText))
            {
                var userExists = UserService.GetUserByEmail(emailText);
                if (userExists != null)
                {
                    ErrorMessage.IsVisible = true;
                    ErrorMessage.Text = "������ ��������� �� Email";

                    await EmailSender.SendNewPasswordAsync(userExists.Email);
                    
                    this.Close();
                }
                else
                {
                    ShowErrorMessage(message: "������������ � ����� Email �� ����������");
                }
            }
            else
            {
                ShowErrorMessage(message: "������������ ������ Email");
            }
        }
        else
        {
            ShowErrorMessage(message: "�� ������� ���� Email");
        }
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage.IsVisible = true;
        ErrorMessage.Text = message;
    }

    private void Back_ButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}