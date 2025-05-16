using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class PersonalGoalsUserControl : UserControl, INotifyPropertyChanged
{
    public bool IsGoalsExists { get; set; }

    private ObservableCollection<Goal> _allGoals = new ObservableCollection<Goal>(); // ��� ���� �����
    public ObservableCollection<Goal> GoalsCollection { get; set; } = new ObservableCollection<Goal>();

    public PersonalGoalsUserControl()
    {
        InitializeComponent();
        LoadGoals();
        DataContext = this;
    }

    private void LoadGoals()
    {
        var goals = PersonalGoalsService.LoadUserPersonalGoals(AuthorizedUser.User);

        foreach (var goal in goals)
        {
            // �������� ������ ��� ���� 
            var tasks = PersonalTasksService.LoadUserTasksByGoal(AuthorizedUser.User, goal);

            // ��������� ������ ��������� ����������
            goal.PendingTasksCount = tasks.Count(t => (bool)!t.IsCompleted);
        }

        _allGoals = new ObservableCollection<Goal>(goals);
        GoalsCollection = new ObservableCollection<Goal>(_allGoals);

        IsGoalsExists = GoalsCollection.Any();
        OnPropertyChanged(nameof(GoalsCollection));
        OnPropertyChanged(nameof(IsGoalsExists));
    }


    //��������� ���� ����� ����� ��������� ���� - ���� ����� ����� ������������ � ������ ���� �����
    private async void AddPersonalGoal_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        AddGoalDialog addGoal = new AddGoalDialog();
        var result = await addGoal.ShowDialog<bool>(contentContainer);

        if (result)
        {
            LoadGoals(); //������������� ������ �����
        }
    }

    private void ShowGoalDetails_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (sender is Button button && button.DataContext is Goal goal)
        {
            contentContainer.ContentControlContainer.Content = new PersonalGoalsDetailUserControl(goal);
        }
    }

    //� ��������� ���������� � ������ ����� �� ������ ������ �������, �� � ������ ����������� ����� 
    //�� ������ "�������� ������� ����������"

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}