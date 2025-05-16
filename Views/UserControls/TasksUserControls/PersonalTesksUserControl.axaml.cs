using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class PersonalTesksUserControl : UserControl, INotifyPropertyChanged
{
    private string selectedSection = "";
    private string prioritySortType = "";
    private string dateSortType = "";

    // ���� ��� �������� ������ ��������
    private ObservableCollection<string> _availableSections = new ObservableCollection<string>();

    private bool _isTasksExists;
    public bool IsTasksExists
    {
        get => _isTasksExists;
        set
        {
            if (_isTasksExists != value)
            {
                _isTasksExists = value;
                OnPropertyChanged(nameof(IsTasksExists));
            }
        }
    }

    public bool IsGoalTask { get; set; } //��� ��������� ����� ������������� ����

    private ObservableCollection<PersonalTask> _allTasks = new ObservableCollection<PersonalTask>(); // ��� ���� �����
    public ObservableCollection<PersonalTask> TasksCollection { get; set; } = new ObservableCollection<PersonalTask>();

    public Goal _goal { get; set; }

    public PersonalTesksUserControl()
    {
        InitializeComponent();
        LoadTasks();

        IsGoalTask = false;
        OnPropertyChanged(nameof(IsGoalTask));

        DataContext = this;
    }

    public PersonalTesksUserControl(Goal goal)
    {
        InitializeComponent();
        _goal = goal;
        IsGoalTask = true;
        LoadTasks();
        OnPropertyChanged(nameof(IsGoalTask));
        DataContext = this;
    }

    private void LoadTasks()
    {
        //���� ���� ������ ����������� ����, ����� ��������� ���������� ������
        if (IsGoalTask)
        {
            _allTasks = new ObservableCollection<PersonalTask>(
            PersonalTasksService.LoadUserTasksByGoal(AuthorizedUser.User, _goal));
        }
        else
        {
            _allTasks = new ObservableCollection<PersonalTask>(
            PersonalTasksService.LoadUserTasks(AuthorizedUser.User));
        }
        
        TasksCollection = new ObservableCollection<PersonalTask>(_allTasks);

        UpdateAvailableSections();
        UpdatePriorityColor();

        IsTasksExists = TasksCollection.Any();
        OnPropertyChanged(nameof(TasksCollection));
        OnPropertyChanged(nameof(IsTasksExists));
    }

    private void LoadCompletedTasks()
    {
        //���� ���� ������ ����������� ����, ����� ��������� ���������� ������
        if (IsGoalTask)
        {
            _allTasks = new ObservableCollection<PersonalTask>(
            PersonalTasksService.LoadUserCompletedTasksByGoal(AuthorizedUser.User, _goal));
        }
        else
        {
            _allTasks = new ObservableCollection<PersonalTask>(
            PersonalTasksService.LoadUserCompletedTasks(AuthorizedUser.User));
        }

        TasksCollection = new ObservableCollection<PersonalTask>(_allTasks);
     

        UpdateAvailableSections();
        UpdatePriorityColor();

        IsTasksExists = TasksCollection.Any();
        OnPropertyChanged(nameof(TasksCollection));
        OnPropertyChanged(nameof(IsTasksExists));
    }

    // ����� ��� ���������� ������ ��������
    private void UpdateAvailableSections()
    {
        _availableSections.Clear();
        _availableSections.Add("��� �������"); // ��������� ������

        var sections = TasksCollection
            .Select(t => t.Section)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        foreach (var section in sections)
        {
            _availableSections.Add(section); // ��������� ������
        }

        SectionComboBox.ItemsSource = _availableSections;
    }

    private void UpdatePriorityColor()
    {
        foreach (var item in TasksCollection)
        {
            if (item.Priority == 1)
            {
                item.PriorityColor = "Red";
            }
            else if (item.Priority == 2)
            {
                item.PriorityColor = "Orange";
            }
            else if (item.Priority == 3)
            {
                item.PriorityColor = "Green";
            }
            else if (item.Priority == 4)
            {
                item.PriorityColor = "Blue";
            }
        }
    }

    private void FilterSortingTasks()
    {
        //  �������� � ������������� �������
        var filteredTasks = _allTasks.ToList();

        // ���������� �� �������
        if (!string.IsNullOrEmpty(selectedSection))
        {
            if (selectedSection != "��� �������")
            {
                filteredTasks = filteredTasks
                    .Where(t => t.Section == selectedSection)
                    .ToList();
            }
        }

        //  ���������� �� ����������
        if (!string.IsNullOrEmpty(prioritySortType))
        {
            filteredTasks = prioritySortType switch
            {
                "�� ����������� ����������" => filteredTasks
                    .OrderByDescending(t => t.Priority)
                    .ToList(),
                "�� �������� ����������" => filteredTasks
                    .OrderBy(t => t.Priority)
                    .ToList(),
                _ => filteredTasks
            };
        }

        // ���������� �� ����
        if (!string.IsNullOrEmpty(dateSortType))
        {
            filteredTasks = dateSortType switch
            {
                "������� ���������" => filteredTasks
                    .OrderBy(t => t.ProbableCompleteDate)
                    .ToList(),
                "������� �������" => filteredTasks
                    .OrderByDescending(t => t.ProbableCompleteDate)
                    .ToList(),
                _ => filteredTasks
            };
        }

        // ��������� ���������
        TasksCollection.Clear();
        foreach (var task in filteredTasks)
        {
            TasksCollection.Add(task);
        }

        OnPropertyChanged(nameof(TasksCollection));
        UpdatePriorityColor();
    }

    private void SectionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string section)
        {
            selectedSection = section;
            FilterSortingTasks();
        }
    }

    private void PrioritySort_SelectionChanged(object sender, RoutedEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo != null)
        {
            var item = combo.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                prioritySortType = item.Content as string;
                FilterSortingTasks();
            }
        }
    }

    private void DateSort_SelectionChanged(object sender, RoutedEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo != null)
        {
            var item = combo.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                dateSortType = item.Content as string;
                FilterSortingTasks();
            }
        }
    }

    private void ShowCurrentTasks_ButtonClick(object sender, RoutedEventArgs e)
    {
        LoadTasks();
    }

    private void ShowCompletedTasks_ButtonClick(object sender, RoutedEventArgs e)
    {
        LoadCompletedTasks();
    }

    private void OpenContextMenuButton_Click(object sender, RoutedEventArgs e) //�������� ����������� ���� �� ������� �� ������ � ����� �������
    {
        if (sender is Button button && button.ContextMenu is ContextMenu contextMenu)
        {
            // ��������� ����������� ���� ������������ ������
            contextMenu.PlacementTarget = button;
            contextMenu.Open(button);
        }
    }

    private async void MarkTaskAsCompleted_ButtonClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var task = (PersonalTask)menuItem.DataContext;

        task.IsCompleted = true;
        task.CompletedAt = DateTime.Now;
        
        await PersonalTasksService.UpdateTask(task);

        //��������� ����� �� ��������� �� ��� �����
        await UserPointsService.CheckPointsData(AuthorizedUser.User, task);

        LoadTasks();
    }

    private async void AddTask_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (_goal != null)
        {
            contentContainer.ContentControlContainer.Content = new AddPersonalTaskUserControl(_goal);
        }
        else
        {
            contentContainer.ContentControlContainer.Content = new AddPersonalTaskUserControl();
        }
    }

    private async void EditTask_ButtonClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var task = (PersonalTask)menuItem.DataContext;

        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new EditPersonalTaskUserControl(task);
    }

    private async void DeleteTask_ContextMenuClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var task = (PersonalTask)menuItem.DataContext;

        PersonalTasksService.DeleteSelectedTask(task);
        TasksCollection.Remove(task);

        // ��������� ���������
        IsTasksExists = TasksCollection.Any();
        OnPropertyChanged(nameof(IsTasksExists));

        // ���� ��� ������ ���� � ������ ���� ������, ��������� UI
        if (IsGoalTask && !IsTasksExists)
        {
            var contentContainer = this.GetVisualRoot() as ContentContainer;
            if (contentContainer != null)
            {
                contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(_goal);
            }
        }
    }

    private async void EditCurrentGoal_ButtonClick(object sender, RoutedEventArgs e)
    {
        //�� ����� ������ ��� ����, ������� ���� ��� �� ��������� �� null
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        EditGoalDialog dialog = new EditGoalDialog(_goal);
        var result = await dialog.ShowDialog<bool>(contentContainer);

        if (result)
        {
            // �������� ����������� ���� �� ���� ������
            var updatedGoal = PersonalGoalsService.GetGoalById(_goal.Id);

            // ������� ������������ PersonalGoalsDetailUserControl � ��������� ���
            var parentControl = this.FindAncestorOfType<PersonalGoalsDetailUserControl>();
            if (parentControl != null)
            {
                parentControl.UpdateGoal(updatedGoal);
            }

            // ����� ��������� ��������� ����� ����
            _goal = updatedGoal;
            OnPropertyChanged(nameof(_goal));
        }
    }

    private async void DeleteCurrentGoal_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        var message = new MessageBox("������ � ����� �������� � ��� ������, �� �������?", true);

        var result = await message.ShowDialog<bool>(contentContainer);
        if (result)
        {
            //������� ��������� ����
            PersonalGoalsService.DeleteGoal(_goal);

            // ������������ � ��������� ������
            contentContainer.ContentControlContainer.Content = new PersonalGoalsUserControl();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}