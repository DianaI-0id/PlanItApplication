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
    public static class PersonalGoalsService
    {
        public static ObservableCollection<Goal> LoadUserPersonalGoals(User user)
        {
            using (var context = new PlanItContext())
            {
                var selectedUser = context.Users.Include(t => t.PersonalTasks).FirstOrDefault(u => u.Id == user.Id);

                var goalsList = context.Goals.Where(u => u.CreatorId == selectedUser.Id && u.IsCompleted == false);
                return new ObservableCollection<Goal>(goalsList);
            }
        }

        public static async Task AddPersonalGoal(Goal goal)
        {
            using (var context = new PlanItContext())
            {
                // Локальная БД
                goal.Id = context.Goals.Any() ? context.Goals.Max(t => t.Id) + 1 : 1;
                context.Goals.Add(goal);
                await context.SaveChangesAsync();
            }
        }

        public static async void UpdatePersonalGoal(Goal goal)
        {
            using (var context = new PlanItContext())
            {
                context.Goals.Update(goal);
                await context.SaveChangesAsync();
            }
        }

        public static async void DeleteGoal(Goal goal)
        {
            using (var context = new PlanItContext())
            {
                context.Goals.Remove(goal);
                await context.SaveChangesAsync();
            }    
        }

        public static Goal GetGoalById(int goalId)
        {
            using (var context = new PlanItContext())
            {
                var goal = context.Goals.FirstOrDefault(g => g.Id == goalId);
                return goal;
            }
        }
    }
}
