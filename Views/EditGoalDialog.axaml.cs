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
            _goal.Description = string.IsNullOrEmpty(GoalDescriptionTextBox.Text) ? "ќписание отсутствует" : GoalDescriptionTextBox.Text;

            PersonalGoalsService.UpdatePersonalGoal(_goal);

            Close(true); //после закрыти€ возвращаем положительный результат дл€ перезагрузки содержимого окна
        }
        else
        {
            ShowErrorMessage(message: "Ќаименование не может быть пустым");
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