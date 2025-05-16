using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Linq;
using System.Threading;

namespace Diploma_Ishchenko;

public partial class SubscribeUserControl : UserControl
{
    private UserControl? _parentUserControl;
    public SubscribeUserControl()
    {
        InitializeComponent();
    }

    //��� ������� ��������� ��������� �� ������ �������� ������� � ��������
    public SubscribeUserControl(UserControl parentUserControl)
    {
        InitializeComponent();
        _parentUserControl = parentUserControl;
        DataContext = this;
    }


    private void CardNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Text;

        // 1. ������� ��������� ���������� �������
        string filteredText = new string(text.Where(char.IsDigit).ToArray());

        // 2. �������� �� 19 ��������
        if (filteredText.Length > 19)
        {
            filteredText = filteredText.Substring(0, 19);
        }

        // 3. ��������� ����� � ������� �������
        if (textBox.Text != filteredText) // ����� �������� ������������ �����
        {
            textBox.Text = filteredText;
            textBox.SelectionStart = filteredText.Length;
        }
    }


    private void MonthTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Text;

        // ������ �����
        if (text.Any(c => !char.IsDigit(c)))
        {
            textBox.Text = new string(text.Where(char.IsDigit).ToArray());
            textBox.SelectionStart = textBox.Text.Length;
        }

        // ����������� ����� �� 2 ����
        if (text.Length > 2)
        {
            textBox.Text = text.Substring(0, 2);
            textBox.SelectionStart = 2;
        }
    }

    private void YearTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Text;

        // ������ �����
        if (text.Any(c => !char.IsDigit(c)))
        {
            textBox.Text = new string(text.Where(char.IsDigit).ToArray());
            textBox.SelectionStart = textBox.Text.Length; // ������������� ������ � �����
        }

        // �������� 2 �����
        if (text.Length > 2)
        {
            textBox.Text = text.Substring(0, 2);
            textBox.SelectionStart = 2;
        }
    }

    private void CVCTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Text;

        // ��������� ������ �����
        if (text.Any(c => !char.IsDigit(c)))
        {
            textBox.Text = new string(text.Where(char.IsDigit).ToArray());
            textBox.SelectionStart = textBox.Text.Length; // ������������� ������ � �����
        }

        // �������� 3 �����
        if (text.Length > 3)
        {
            textBox.Text = text.Substring(0, 3);
            textBox.SelectionStart = 3;
        }
    }

    private async void SubscribeButton_Click(object sender, RoutedEventArgs e)
    {
        string cardNumber = CardNumberTextBox.Text;
        string monthText = MonthTextBox.Text; // �������� �����, � �� ����� �����
        string year = YearTextBox.Text;
        string cvc = CVCTextBox.Text;

        // �������� ������ �����
        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(monthText) || string.IsNullOrEmpty(year) || string.IsNullOrEmpty(cvc))
        {
            ShowErrorMessage("��������� ��� ����");
            return;
        }

        // �������� ����� ������ �����
        if (cardNumber.Length < 13 || cardNumber.Length > 19)
        {
            ShowErrorMessage("����� ����� ������ ��������� �� 13 �� 19 ����");
            return;
        }

        // �������� CVC
        if (cvc.Length != 3)
        {
            ShowErrorMessage("CVC ������ �������� �� 3 ����");
            return;
        }

        // ��������� �������� ������ �����
        if (!int.TryParse(monthText, out int month) || month < 1 || month > 12)
        {
            ShowErrorMessage("������������ ����� (1-12)");
            return;
        }

        // �������� ����� �������� �����
        if (!ValidateExpiryDate())
            return;

        // ���� ��� �������� ��������
        ErrorMessageTextBlock.IsVisible = false;

        //������� messageBox � ����������� �� �������� ��������
        //��������� ������ ������������ � ��������

        var contentContainer = this.GetVisualRoot() as ContentContainer;

        var user = UserService.GetUserByEmail(AuthorizedUser.User.Email);
        if (user != null)
        {
            user.HasActiveSubscription = true;
            user.SubscriptionStartDate = DateTime.Now;
            user.SubscriptionEndDate = DateTime.Now.AddDays(30);

            await UserService.UpdateUser(user);
            await SubscriptionService.AddSubscriptionData(user);
            //���� �� ����� �������� ���������� ���� �� � ��������

            //�������� �������� ������������
            AuthorizedUser.User = user;
            AuthorizedUser.User.HasActiveSubscription = true;

            contentContainer.ContentControlContainer.Content = new UserProfileUserControl();
        }

        //������������ ���������
        this.IsEnabled = false;

        Thread.Sleep(1000); //���� ���� ������� ����� �������

        var messageBox = new MessageBox("�������� ������� ���������!", false);
        await messageBox.ShowDialog(contentContainer);
    }

    private bool ValidateExpiryDate()
    {
        if (int.TryParse(MonthTextBox.Text, out int month) &&
            int.TryParse(YearTextBox.Text, out int year))
        {
            var currentYear = DateTime.Now.Year % 100;
            var currentMonth = DateTime.Now.Month;

            if (year < currentYear || (year == currentYear && month < currentMonth))
            {
                ShowErrorMessage("���� �������� ����� �����");
                return false;
            }
            return true;
        }
        ShowErrorMessage("������������ ������ ����");
        return false;
    }

    //private bool ValidateLuhn(string cardNumber)
    //{
    //    int sum = 0;
    //    bool alternate = false;

    //    for (int i = cardNumber.Length - 1; i >= 0; i--)
    //    {
    //        int digit = int.Parse(cardNumber[i].ToString());
    //        if (alternate)
    //        {
    //            digit *= 2;
    //            if (digit > 9)
    //            {
    //                digit -= 9;
    //            }
    //        }
    //        sum += digit;
    //        alternate = !alternate;
    //    }

    //    bool isValid = sum % 10 == 0;
    //    if (!isValid)
    //    {
    //        ShowErrorMessage("������������ ����� �����");
    //    }
    //    return isValid;
    //}

    private void Back_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = _parentUserControl;
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessageTextBlock.Text = message;
        ErrorMessageTextBlock.IsVisible = true;
    }
}
