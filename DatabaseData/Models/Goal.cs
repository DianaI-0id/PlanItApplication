using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Goal
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? CreatorId { get; set; }

    //public int? AdminId { get; set; }

    public bool? IsCompleted { get; set; }

    //public bool? IsGroupGoal { get; set; }

    public DateTime? CreatedAt { get; set; }

    //public string? InviteCode { get; set; }

    //public virtual User? Admin { get; set; }

    public virtual User? Creator { get; set; }

    [NotMapped]
    public int PendingTasksCount { get; set; } //количество задач, ожидающих выполнения. Отображается в кружке в верстке

    //public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();

    //public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<PersonalTask> PersonalTasks { get; set; } = new List<PersonalTask>();
}
