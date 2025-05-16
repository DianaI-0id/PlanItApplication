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

    // Поле для хранения актуальных задач пользователя
    private List<PersonalTask> _userTasks = new List<PersonalTask>();

    public ObservableCollection<ISeries> PieSeries { get; } = new ObservableCollection<ISeries>();

    // Данные пользователя (заполни при открытии окна)
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
        using (var dbContext = new PlanItContext()) // замените на ваш контекст
        {
            // Загружаем пользователя с задачами (PersonalTasks) через Include (жадная загрузка)
            var userWithTasks = dbContext.Users
                .Include(u => u.PersonalTasks) // загрузка связанных задач
                .FirstOrDefault(u => u.Id == _user.Id);

            if (userWithTasks != null && userWithTasks.PersonalTasks != null)
            {
                // Обновляем локальный список задач
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
        // Обновляем список задач из базы (или из актуального источника)
        RefreshUserTasks();

        RefreshUserWithRelatedData();

        // Определяем границы сегодняшнего дня
        DateTime startToday = DateTime.Today; // Сегодня в 00:00:00
        DateTime endToday = startToday.AddDays(1); // Завтра в 00:00:00

        // Считаем количество задач, которые выполнены сегодня
        int completedToday = _userTasks.Count(task =>
            task.IsCompleted == true &&
            task.CompletedAt.HasValue &&
            task.CompletedAt.Value >= startToday &&
            task.CompletedAt.Value < endToday);

        // Обновляем текст для отображения
        TodayStatsText = $"Задач выполнено сегодня: {completedToday}";
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
        // Обновляем список задач
        RefreshUserTasks();

        RefreshUserWithRelatedData();

        // Определяем границы периода
        DateTime periodStart = DateTime.Today.AddDays(-days + 1); // Начало периода (включительно)
        DateTime periodEnd = DateTime.Today.AddDays(1); // Конец периода (не включительно, завтра 00:00)

        // Считаем количество задач, выполненных за период
        int tasksCompleted = _userTasks.Count(task =>
            task.IsCompleted == true &&
            task.CompletedAt.HasValue &&
            task.CompletedAt.Value >= periodStart &&
            task.CompletedAt.Value < periodEnd);

        int tasksInProgress = _user.PersonalTasks.Count(task =>
            task.IsCompleted == false &&
            task.CreatedAt.HasValue &&
            task.CreatedAt.Value < periodEnd); // учитываем все задачи, созданные до конца периода

        // Аналогично считаем количество просроченных задач за период
        int tasksOverdueCompleted = _userTasks.Count(task =>
            task.IsCompleted == true &&                        // задача выполнена
            task.CompletedAt.HasValue &&                        // есть дата выполнения
            task.ProbableCompleteDate.HasValue &&              // есть предполагаемая дата
            task.CompletedAt.Value > task.ProbableCompleteDate.Value &&  // выполнена позже предполагаемой
            task.CompletedAt.Value >= periodStart &&           // выполнена в выбранном периоде
            task.CompletedAt.Value < periodEnd);

        // Невыполненные и уже просроченные
        int tasksOverdueUncompleted = _user.PersonalTasks.Count(task =>
            task.IsCompleted == false &&
            task.ProbableCompleteDate.HasValue &&
            task.ProbableCompleteDate.Value < DateTime.Now && // Уже просрочена
            task.CreatedAt.HasValue &&
            task.CreatedAt.Value < periodEnd); // Задача создана до конца периода

        int tasksOverdue = tasksOverdueCompleted + tasksOverdueUncompleted;

        // Считаем количество достигнутых целей за период
        int goalsCompleted = _user.GoalCreators.Count(goal =>
            goal.IsCompleted == true &&
            goal.CreatedAt.HasValue &&
            goal.CreatedAt.Value >= periodStart &&
            goal.CreatedAt.Value < periodEnd);

        // Считаем количество постов за период
        int postsWritten = _user.Posts.Count(post =>
            post.CreatedAt.HasValue &&
            post.CreatedAt.Value >= periodStart &&
            post.CreatedAt.Value < periodEnd);

        // Считаем сумму баллов за период
        int pointsEarned = _user.PointsHistories
            .Where(ph => ph.CreatedAt.HasValue &&
                         ph.CreatedAt.Value >= periodStart &&
                         ph.CreatedAt.Value < periodEnd)
            .Sum(ph => ph.Amount);

        // Запускаем анимацию для каждого значения (предполагается, что AnimateNumber реализован)
        await AnimateNumber(nameof(AnimatedTasksCompleted), tasksCompleted);
        await AnimateNumber(nameof(AnimatedTasksOverdue), tasksOverdue);
        await AnimateNumber(nameof(AnimatedGoalsCompleted), goalsCompleted);
        await AnimateNumber(nameof(AnimatedPostsWritten), postsWritten);
        await AnimateNumber(nameof(AnimatedPointsEarned), pointsEarned);
        await AnimateNumber(nameof(AnimatedTasksInProgress), tasksInProgress);

        // Загружаем графики за период
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
                Name = "Нет данных",
                Fill = new SolidColorPaint(SKColors.LightGray),
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsFormatter = point => "Нет данных"
            });
            OnPropertyChanged(nameof(PieSeries));
            return;
        }

        if (tasksCompleted > 0)
        {
            PieSeries.Add(new PieSeries<int>
            {
                Values = new[] { tasksCompleted },
                Name = "Выполненные задачи",
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
                Name = "Просрочено задач",
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
                Name = "В процессе",
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
            // Получаем все даты, когда пользователь получил очки за задачи (TaskId != null)
            var taskDates = dbContext.PointsHistories
                .Where(ph => ph.UserId == userId && ph.TaskId != null && ph.CreatedAt.HasValue)
                .Select(ph => ph.CreatedAt.Value.Date)  // Берём только дату без времени
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (taskDates.Count == 0)
                return 0;

            int consecutiveDays = 1;
            DateTime currentDate = taskDates[0];

            for (int i = 1; i < taskDates.Count; i++)
            {
                // Проверяем, что следующий день в списке - ровно на 1 день меньше текущего
                if ((currentDate - taskDates[i]).Days == 1)
                {
                    consecutiveDays++;
                    currentDate = taskDates[i];
                }
                else if ((currentDate - taskDates[i]).Days > 1)
                {
                    // Пропуск дней - прерываем подсчёт
                    break;
                }
                else
                {
                    // Дублирующиеся даты или неправильный порядок - можно игнорировать
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