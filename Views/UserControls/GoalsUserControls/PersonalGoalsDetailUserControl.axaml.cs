using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Diploma_Ishchenko.DatabaseData.Models;
using System.ComponentModel;

namespace Diploma_Ishchenko;

public partial class PersonalGoalsDetailUserControl : UserControl, INotifyPropertyChanged
{
    private PersonalTesksUserControl _personalTasksControl;

    public Goal _goal { get; set; }
    //public bool _IsTasksExists { get; set; } //���� ����� � ���� ���, �� �� ������� ��������������� ������
    //�������� �������� �� ���� PersonalTesks

    public PersonalGoalsDetailUserControl()
    {
        InitializeComponent();
    }

    public PersonalGoalsDetailUserControl(Goal goal)
    {
        InitializeComponent();
        _goal = goal;
        LoadGoalTasks();
        DataContext = this;
    }

    //� �������� ������� ��������� ������ �����, ������������� ������ ����
    private void LoadGoalTasks()
    {
        _personalTasksControl = new PersonalTesksUserControl(_goal);
        GoalTasksContentControl.Content = _personalTasksControl;
    }

    // ����� ��� ���������� ������
    public void UpdateGoal(Goal updatedGoal)
    {
        _goal = updatedGoal;
        OnPropertyChanged(nameof(_goal));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}