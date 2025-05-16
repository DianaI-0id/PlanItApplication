using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class PersonalTask : INotifyPropertyChanged
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? GoalId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Section { get; set; }

    public bool IsOverdue
    {
        get
        {
            // Проверяем, что задача не выполнена и дата выполнения установлена
            if (IsCompleted == true || ProbableCompleteDate == null)
                return false;

            // Сравниваем дату выполнения с текущей датой
            return ProbableCompleteDate.Value < DateTime.Now;
        }
    }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? Priority { get; set; }

    [NotMapped]
    public string PriorityColor { get; set; }

    public DateTime? ProbableCompleteDate { get; set; }

    public virtual Goal? Goal { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual User? User { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
