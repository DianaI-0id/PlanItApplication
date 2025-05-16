using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Diploma_Ishchenko;

public partial class AddPersonalTaskUserControl : UserControl, INotifyPropertyChanged
{
    private int priority = 0;
    private DateTimeOffset selectedDate;
    private TimeSpan selectedTime;

    public DateTimeOffset SelectedDate
    {
        get => selectedDate;
        set
        {
            selectedDate = value;
            OnPropertyChanged(nameof(SelectedDate));
            OnPropertyChanged(nameof(FullDateTime));
        }
    }

    public TimeSpan SelectedTime
    {
        get => selectedTime;
        set
        {
            selectedTime = value;
            OnPropertyChanged(nameof(SelectedTime));
            OnPropertyChanged(nameof(FullDateTime));
        }
    }

    // �������� ��� ����������� ������ ���� � �������
    public DateTime FullDateTime => SelectedDate.Date + SelectedTime;

    public Goal _goal { get; set; }
    public bool IsGoalTask { get; set; }

    public AddPersonalTaskUserControl()
    {
        InitializeComponent();
        
        IsGoalTask = false;
        OnPropertyChanged(nameof(IsGoalTask));
        SelectedDate = DateTimeOffset.Now; // ��������� �� ���������
        SelectedTime = DateTime.Now.TimeOfDay; // ������� ����� �� ���������

        DataContext = this;
    }

    //�������� ����������� ������� ����� ��������� ���� ��� �������� ������ � ���
    public AddPersonalTaskUserControl(Goal goal)
    {
        InitializeComponent();

        _goal = goal;
        IsGoalTask = true;
        OnPropertyChanged(nameof(IsGoalTask));
        SelectedDate = DateTimeOffset.Now; // ��������� �� ���������
        SelectedTime = DateTime.Now.TimeOfDay; // ������� ����� �� ���������

        DataContext = this;
    }

    private async void AddTask_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TaskTitleTextBox.Text) &&
            !string.IsNullOrEmpty(SectionTextBox.Text) &&
            priority > 0)
        {
            // ����������� ���� � �����
            var completeDateTime = SelectedDate.Date + SelectedTime;

            var task = new PersonalTask
            {
                UserId = AuthorizedUser.User.Id,
                Title = TaskTitleTextBox.Text,
                Description = string.IsNullOrEmpty(TaskDescriptionTextBox.Text) ? "�������� �����������" : TaskDescriptionTextBox.Text,
                Section = SectionTextBox.Text,
                ProbableCompleteDate = completeDateTime, // ���������� ��������������� ���� � �����
                Priority = priority,
                GoalId = _goal != null ? _goal.Id : null
            };

            await PersonalTasksService.AddTask(task);

            var contentContainer = this.GetVisualRoot() as ContentContainer;

            if (_goal != null)
            {
                contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(_goal);
            }
            else
            {
                contentContainer.ContentControlContainer.Content = new PersonalTesksUserControl();
            }
        }
        else
        {
            ShowErrorMessage(message: "�� ��� ���� ���������");
        }
    }

    private void Return_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (_goal != null)
        {
            contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(_goal);
        }
        else
        {
            contentContainer.ContentControlContainer.Content = new PersonalTesksUserControl();
        }
    }

    private async void SelectPriority_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo != null && combo.SelectedItem != null)
        {
            var comboItem = combo.SelectedItem as ComboBoxItem;
            if (comboItem != null)
            {
                if (int.TryParse(comboItem.Content.ToString(), out int item))
                {
                    priority = item;
                }
                else
                {
                    // ��������� ������, ����� SelectedItem �� �������� ����� ������
                    priority = 1; // �������� �� ���������
                }
            }
        }
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage.IsVisible = true;
        ErrorMessage.Text = message;
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) 
    { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}