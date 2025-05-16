using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using Diploma_Ishchenko.DatabaseData.Context;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using ShimSkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore.Painting;
using SkiaSharp;
using Diploma_Ishchenko.DatabaseData.Models;
using System.Collections.Generic;
using System.Drawing;
using Diploma_Ishchenko.Services.PostgresServices;


namespace Diploma_Ishchenko;

public partial class AnalyticsUserControl : UserControl, INotifyPropertyChanged
{
    public string TodayStatsText { get; set; }
    public int AnimatedTasksCompleted { get; set; }
    public int AnimatedTasksOverdue { get; set; }
    public int AnimatedGoalsCompleted { get; set; }
    public int AnimatedPostsWritten { get; set; }
    public int AnimatedPointsEarned { get; set; }
    public int AnimatedTasksInProgress { get; set; }
    public int ConsecutiveDays { get; set; }

    // ���� ��� �������� ���������� ����� ������������
    private List<PersonalTask> _userTasks = new List<PersonalTask>();

    public ObservableCollection<ISeries> PieSeries { get; } = new ObservableCollection<ISeries>();

    // ������ ������������ (������� ��� �������� ����)
    private User _user;
    private int _periodDays = 7;

    public AnalyticsUserControl()
    {
        InitializeComponent();

        _user = UserService.GetUserByEmail(AuthorizedUser.User.Email);
        DataContext = this; 

        LoadTodayStats();
        LoadPeriodStats(_periodDays);
    }
    private void RefreshUserWithRelatedData()
    {
        using (var dbContext = new PlanItContext())
        {
            _user = dbContext.Users
                .Include(u => u.PersonalTasks)
                .Include(u => u.GoalCreators)
                .Include(u => u.Posts)
                .Include(u => u.PointsHistories)
                .FirstOrDefault(u => u.Id == _user.Id);
        }
    }

    private void RefreshUserTasks()
    {
        using (var dbContext = new PlanItContext()) // �������� �� ��� ��������
        {
            // ��������� ������������ � �������� (PersonalTasks) ����� Include (������ ��������)
            var userWithTasks = dbContext.Users
                .Include(u => u.PersonalTasks) // �������� ��������� �����
                .FirstOrDefault(u => u.Id == _user.Id);

            if (userWithTasks != null && userWithTasks.PersonalTasks != null)
            {
                // ��������� ��������� ������ �����
                _userTasks = userWithTasks.PersonalTasks.ToList();
            }
            else
            {
                _userTasks = new List<PersonalTask>();
            }
        }
    }

    private void LoadTodayStats()
    {
        // ��������� ������ ����� �� ���� (��� �� ����������� ���������)
        RefreshUserTasks();

        RefreshUserWithRelatedData();

        // ���������� ������� ������������ ���
        DateTime startToday = DateTime.Today; // ������� � 00:00:00
        DateTime endToday = startToday.AddDays(1); // ������ � 00:00:00

        // ������� ���������� �����, ������� ��������� �������
        int completedToday = _userTasks.Count(task =>
            task.IsCompleted == true &&
            task.CompletedAt.HasValue &&
            task.CompletedAt.Value >= startToday &&
            task.CompletedAt.Value < endToday);

        // ��������� ����� ��� �����������
        TodayStatsText = $"����� ��������� �������: {completedToday}";
        OnPropertyChanged(nameof(TodayStatsText));
    }


    private void PeriodButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int days))
        {
            _periodDays = days;
            LoadPeriodStats(_periodDays);
        }
    }

    private async void LoadPeriodStats(int days)
    {
        // ��������� ������ �����
        RefreshUserTasks();

        RefreshUserWithRelatedData();

        // ���������� ������� �������
        DateTime periodStart = DateTime.Today.AddDays(-days + 1); // ������ ������� (������������)
        DateTime periodEnd = DateTime.Today.AddDays(1); // ����� ������� (�� ������������, ������ 00:00)

        // ������� ���������� �����, ����������� �� ������
        int tasksCompleted = _userTasks.Count(task =>
            task.IsCompleted == true &&
            task.CompletedAt.HasValue &&
            task.CompletedAt.Value >= periodStart &&
            task.CompletedAt.Value < periodEnd);

        int tasksInProgress = _user.PersonalTasks.Count(task =>
            task.IsCompleted == false &&
            task.CreatedAt.HasValue &&
            task.CreatedAt.Value < periodEnd); // ��������� ��� ������, ��������� �� ����� �������

        // ���������� ������� ���������� ������������ ����� �� ������
        int tasksOverdueCompleted = _userTasks.Count(task =>
            task.IsCompleted == true &&                        // ������ ���������
            task.CompletedAt.HasValue &&                        // ���� ���� ����������
            task.ProbableCompleteDate.HasValue &&              // ���� �������������� ����
            task.CompletedAt.Value > task.ProbableCompleteDate.Value &&  // ��������� ����� ��������������
            task.CompletedAt.Value >= periodStart &&           // ��������� � ��������� �������
            task.CompletedAt.Value < periodEnd);

        // ������������� � ��� ������������
        int tasksOverdueUncompleted = _user.PersonalTasks.Count(task =>
            task.IsCompleted == false &&
            task.ProbableCompleteDate.HasValue &&
            task.ProbableCompleteDate.Value < DateTime.Now && // ��� ����������
            task.CreatedAt.HasValue &&
            task.CreatedAt.Value < periodEnd); // ������ ������� �� ����� �������

        int tasksOverdue = tasksOverdueCompleted + tasksOverdueUncompleted;

        // ������� ���������� ����������� ����� �� ������
        int goalsCompleted = _user.GoalCreators.Count(goal =>
            goal.IsCompleted == true &&
            goal.CreatedAt.HasValue &&
            goal.CreatedAt.Value >= periodStart &&
            goal.CreatedAt.Value < periodEnd);

        // ������� ���������� ������ �� ������
        int postsWritten = _user.Posts.Count(post =>
            post.CreatedAt.HasValue &&
            post.CreatedAt.Value >= periodStart &&
            post.CreatedAt.Value < periodEnd);

        // ������� ����� ������ �� ������
        int pointsEarned = _user.PointsHistories
            .Where(ph => ph.CreatedAt.HasValue &&
                         ph.CreatedAt.Value >= periodStart &&
                         ph.CreatedAt.Value < periodEnd)
            .Sum(ph => ph.Amount);

        // ��������� �������� ��� ������� �������� (��������������, ��� AnimateNumber ����������)
        await AnimateNumber(nameof(AnimatedTasksCompleted), tasksCompleted);
        await AnimateNumber(nameof(AnimatedTasksOverdue), tasksOverdue);
        await AnimateNumber(nameof(AnimatedGoalsCompleted), goalsCompleted);
        await AnimateNumber(nameof(AnimatedPostsWritten), postsWritten);
        await AnimateNumber(nameof(AnimatedPointsEarned), pointsEarned);
        await AnimateNumber(nameof(AnimatedTasksInProgress), tasksInProgress);

        // ��������� ������� �� ������
        LoadCharts(tasksCompleted, tasksOverdue, tasksInProgress);
    }

    private async Task AnimateNumber(string property, int target)
    {
        int current = 0;
        int step = Math.Max(1, target / 20);
        while (current < target)
        {
            current += step;
            if (current > target) current = target;
            SetProperty(property, current);
            await Task.Delay(100);
        }
        SetProperty(property, target);
    }

    private void SetProperty(string propertyName, int value)
    {
        switch (propertyName)
        {
            case nameof(AnimatedTasksCompleted): AnimatedTasksCompleted = value; break;
            case nameof(AnimatedTasksOverdue): AnimatedTasksOverdue = value; break;
            case nameof(AnimatedGoalsCompleted): AnimatedGoalsCompleted = value; break;
            case nameof(AnimatedPostsWritten): AnimatedPostsWritten = value; break;
            case nameof(AnimatedPointsEarned): AnimatedPointsEarned = value; break;
        }
        OnPropertyChanged(propertyName);
    }


    private void LoadCharts(int tasksCompleted, int tasksOverdue, int tasksInProgress)
    {
        var total = tasksCompleted + tasksOverdue + tasksInProgress;
        ConsecutiveDays = CalculateConsecutiveDaysWithTasks(_user.Id);
        OnPropertyChanged(nameof(ConsecutiveDays));

        PieSeries.Clear();

        if (total == 0)
        {
            PieSeries.Add(new PieSeries<int>
            {
                Values = new[] { 1 },
                Name = "��� ������",
                Fill = new SolidColorPaint(SKColors.LightGray),
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsFormatter = point => "��� ������"
            });
            OnPropertyChanged(nameof(PieSeries));
            return;
        }

        if (tasksCompleted > 0)
        {
            PieSeries.Add(new PieSeries<int>
            {
                Values = new[] { tasksCompleted },
                Name = "����������� ������",
                Fill = new SolidColorPaint(SKColors.Blue),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{tasksCompleted}/{total} ({(double)tasksCompleted / total * 100:N2}%)"
            });
        }

        if (tasksOverdue > 0)
        {
            PieSeries.Add(new PieSeries<int>
            {
                Values = new[] { tasksOverdue },
                Name = "���������� �����",
                Fill = new SolidColorPaint(SKColors.Red),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{tasksOverdue}/{total} ({(double)tasksOverdue / total * 100:N2}%)"
            });
        }

        if (tasksInProgress > 0)
        {
            PieSeries.Add(new PieSeries<int>
            {
                Values = new[] { tasksInProgress },
                Name = "� ��������",
                Fill = new SolidColorPaint(SKColors.Orange),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{tasksInProgress}/{total} ({(double)tasksInProgress / total * 100:N2}%)"
            });
        }

        OnPropertyChanged(nameof(PieSeries));
    }

    private int CalculateConsecutiveDaysWithTasks(int userId)
    {
        using (var dbContext = new PlanItContext())
        {
            // �������� ��� ����, ����� ������������ ������� ���� �� ������ (TaskId != null)
            var taskDates = dbContext.PointsHistories
                .Where(ph => ph.UserId == userId && ph.TaskId != null && ph.CreatedAt.HasValue)
                .Select(ph => ph.CreatedAt.Value.Date)  // ���� ������ ���� ��� �������
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (taskDates.Count == 0)
                return 0;

            int consecutiveDays = 1;
            DateTime currentDate = taskDates[0];

            for (int i = 1; i < taskDates.Count; i++)
            {
                // ���������, ��� ��������� ���� � ������ - ����� �� 1 ���� ������ ��������
                if ((currentDate - taskDates[i]).Days == 1)
                {
                    consecutiveDays++;
                    currentDate = taskDates[i];
                }
                else if ((currentDate - taskDates[i]).Days > 1)
                {
                    // ������� ���� - ��������� �������
                    break;
                }
                else
                {
                    // ������������� ���� ��� ������������ ������� - ����� ������������
                    currentDate = taskDates[i];
                }
            }

            return consecutiveDays;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}