using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;

namespace Diploma_Ishchenko;

public partial class AddGoalDialog : Window
{
    public AddGoalDialog()
    {
        InitializeComponent();
    }

    private async void AddPersonalGoal_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(GoalTitleTextBox.Text))
        {
            var personalGoal = new Goal
            {
                Title = GoalTitleTextBox.Text,
                Description = string.IsNullOrEmpty(GoalDescriptionTextBox.Text) ? "�������� �����������" : GoalDescriptionTextBox.Text,
                CreatorId = AuthorizedUser.User.Id
            };

            await PersonalGoalsService.AddPersonalGoal(personalGoal);

            Close(true); //����� �������� ���������� ������������� ��������� ��� ������������ ����������� ����
        }
        else
        {
            ShowErrorMessage(message: "������������ �� ����� ���� ������");
        }
    }

    private void Close_ButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close(false);
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage.IsVisible = true;
        ErrorMessage.Text = message;
    }
}