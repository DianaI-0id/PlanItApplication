using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.PostgresServices
{
    public static class PersonalTasksService
    {
        public static ObservableCollection<PersonalTask> LoadUserTasks(User user)
        {
            using (var context = new PlanItContext())
            {
                var tasksList = context.PersonalTasks
                    .Where(u => u.UserId == user.Id &&
                           u.IsCompleted == false &&
                           u.GoalId == null).ToList();

                return new ObservableCollection<PersonalTask>(tasksList);
            }
        }

        public static ObservableCollection<PersonalTask> LoadUserCompletedTasks(User user)
        {
            using (var context = new PlanItContext())
            {
                var tasksList = context.PersonalTasks
                    .Where(u => u.UserId == user.Id &&
                           u.IsCompleted == true &&
                           u.GoalId == null).ToList();

                return new ObservableCollection<PersonalTask>(tasksList);
            }
        }

        public static ObservableCollection<PersonalTask> LoadUserTasksByGoal(User user, Goal goal)
        {
            using (var context = new PlanItContext())
            {
                var selectedUser = context.Users.Include(t => t.PersonalTasks).FirstOrDefault(u => u.Id == user.Id);

                var tasksList = context.PersonalTasks
                    .Where(u => u.UserId == selectedUser.Id && u.IsCompleted == false && u.GoalId == goal.Id);

                return new ObservableCollection<PersonalTask>(tasksList);
            }
        }

        public static ObservableCollection<PersonalTask> LoadUserCompletedTasksByGoal(User user, Goal goal)
        {
            using (var context = new PlanItContext())
            {
                var selectedUser = context.Users.Include(t => t.PersonalTasks).FirstOrDefault(u => u.Id == user.Id);

                var tasksList = context.PersonalTasks
                    .Where(u => u.UserId == selectedUser.Id && u.IsCompleted == true && u.GoalId == goal.Id);

                return new ObservableCollection<PersonalTask>(tasksList);
            }
        }

        public static async Task AddTask(PersonalTask task)
        {
            using (var context = new PlanItContext())
            {
                // Локальная БД
                task.Id = context.PersonalTasks.Any() ? context.PersonalTasks.Max(t => t.Id) + 1 : 1;
                context.PersonalTasks.Add(task);
                await context.SaveChangesAsync();
            }
        }

        public static async Task UpdateTask(PersonalTask task)
        {
            using (var context = new PlanItContext())
            {
                // Локальная БД
                context.PersonalTasks.Update(task);
                await context.SaveChangesAsync();
            }
        }

        public static async void DeleteSelectedTask(PersonalTask task)
        {
            using (var context = new PlanItContext())
            {
                // Локальная БД
                context.PersonalTasks.Remove(task);
                await context.SaveChangesAsync();
            }
        }
    }
}
