using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;

namespace Diploma_Ishchenko;

public partial class EditGoalDialog : Window
{
    public Goal _goal { get; set; }
    public EditGoalDialog()
    {
        InitializeComponent();
    }

    public EditGoalDialog(Goal goal)
    {
        InitializeComponent();
        _goal = goal;
        DataContext = this;
    }

    private async void EditPersonalGoal_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(GoalTitleTextBox.Text))
        {
            _goal.Title = GoalTitleTextBox.Text;
            _goal.Description = string.IsNullOrEmpty(GoalDescriptionTextBox.Text) ? "�������� �����������" : GoalDescriptionTextBox.Text;

            PersonalGoalsService.UpdatePersonalGoal(_goal);

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