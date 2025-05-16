using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.ComponentModel;

namespace Diploma_Ishchenko;

public partial class EditPersonalTaskUserControl : UserControl, INotifyPropertyChanged
{
    public PersonalTask Task { get; set; }
    public Goal TaskGoal { get; set; }
    public bool IsGoalTask { get; set; }

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

    // Свойство для отображения полной даты и времени
    public DateTime FullDateTime => SelectedDate.Date + SelectedTime;

    private int priority = 0;

    public EditPersonalTaskUserControl()
    {
        InitializeComponent();
        IsGoalTask = false;
    }

    public EditPersonalTaskUserControl(PersonalTask task)
    {
        InitializeComponent();
        Task = task;
        CheckGoalIfExistsAndLoad();

        // Устанавливаем дату и время из задачи
        SelectedDate = (DateTimeOffset)Task.ProbableCompleteDate;
        SelectedTime = Task.ProbableCompleteDate.Value.TimeOfDay;

        // Устанавливаем приоритет
        priority = (int)Task.Priority;

        DataContext = this;
    }

    private void CheckGoalIfExistsAndLoad()
    {
        if (Task.GoalId != null)
        {
            IsGoalTask = true;
            TaskGoal = PersonalGoalsService.GetGoalById((int)Task.GoalId);
        }
        else
        {
            IsGoalTask = false;
        }
        OnPropertyChanged(nameof(IsGoalTask));

        
    }

    private async void EditTask_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TaskTitleTextBox.Text) &&
            !string.IsNullOrEmpty(SectionTextBox.Text) &&
            priority > 0)
        {
            // Комбинируем дату и время
            var completeDateTime = SelectedDate.Date + SelectedTime;

            Task.Title = TaskTitleTextBox.Text;
            Task.Description = string.IsNullOrEmpty(TaskDescriptionTextBox.Text) ? "Описание отсутствует" : TaskDescriptionTextBox.Text;
            Task.ProbableCompleteDate = completeDateTime; // Используем комбинированную дату и время
            Task.Section = SectionTextBox.Text;
            Task.Priority = priority;

            // Сохраняем изменения
            await PersonalTasksService.UpdateTask(Task);

            // Получаем корневой контейнер
            var contentContainer = this.GetVisualRoot() as ContentContainer;

            // Проверяем, привязана ли задача к цели
            if (Task.GoalId != null)
            {
                // Если привязана - переходим в детали цели
                var goal = PersonalGoalsService.GetGoalById((int)Task.GoalId);
                contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(goal);
            }
            else
            {
                // Если не привязана - переходим в общий список задач
                contentContainer.ContentControlContainer.Content = new PersonalTesksUserControl();
            }
        }
        else
        {
            ShowErrorMessage(message: "Не все поля заполнены");
        }
    }

    private void SelectPriority_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo != null)
        {
            var item = combo.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                if (int.TryParse(item.Content.ToString(), out int priorityValue))
                {
                    priority = priorityValue;
                }
                else
                {
                    priority = 1;
                }
            }
        }
    }
    
    private void Return_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (Task.GoalId != null)
        {
            var goal = PersonalGoalsService.GetGoalById((int)Task.GoalId);
            contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(goal);
        }
        else
        {
            contentContainer.ContentControlContainer.Content = new PersonalTesksUserControl();
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