using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class User : INotifyPropertyChanged
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly? Birthdate { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? GoogleId { get; set; }

    public string? TelegramId { get; set; }

    public string Nickname { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsAdmin { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public bool? HasActiveSubscription { get; set; }

    public string? Biography { get; set; }

    public string? Userphoto { get; set; }

    private bool _isSubscription;

    [NotMapped]
    public bool IsSubscription
    {
        get => _isSubscription;
        set
        {
            if (_isSubscription != value)
            {
                _isSubscription = value;
                OnPropertyChanged(nameof(IsSubscription));
            }
        }
    }

    [NotMapped] 
    public Bitmap? AvatarImage { get; set; }

    [NotMapped]
    private bool _isNotCurrentSubscriptedUser;

    [NotMapped]
    public bool IsNotCurrentSubscriptedUser
    {
        get => _isNotCurrentSubscriptedUser;
        set
        {
            if (_isNotCurrentSubscriptedUser != value)
            {
                _isNotCurrentSubscriptedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotCurrentSubscriptedUser));
                OnPropertyChanged(nameof(IsSubscribedUser));
            }
        }
    }

    [NotMapped]
    private bool _isCurrentUser;

    [NotMapped]
    public bool IsCurrentUser
    {
        get => _isCurrentUser;
        set
        {
            if (_isCurrentUser != value)
            {
                _isCurrentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCurrentUser));
                OnPropertyChanged(nameof(IsSubscribedUser));
            }
        }
    }

    [NotMapped]
    private bool _isCurrentUserAdmin;

    [NotMapped]
    public bool IsCurrentUserAdmin
    {
        get => _isCurrentUserAdmin;
        set
        {
            if (_isCurrentUserAdmin != value)
            {
                _isCurrentUserAdmin = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(IsCurrentUserAdmin));
            }
        }
    }

    [NotMapped]
    private bool _isCurrentUserNotAdmin;

    [NotMapped]
    public bool IsCurrentUserNotAdmin
    {
        get => _isCurrentUserNotAdmin;
        set
        {
            if (_isCurrentUserNotAdmin != value)
            {
                _isCurrentUserNotAdmin = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(IsCurrentUserNotAdmin));
            }
        }
    }

    [NotMapped]
    public bool IsSubscribedUser => !IsCurrentUser && !IsNotCurrentSubscriptedUser;

    [NotMapped]
    private int _subscriptionsCount;
    [NotMapped]
    public int SubscriptionsCount
    {
        get => _subscriptionsCount;
        set
        {
            if (_subscriptionsCount != value)
            {
                _subscriptionsCount = value;
                OnPropertyChanged();
            }
        }
    }

    [NotMapped]
    private int _subscribersCount;
    [NotMapped]
    public int SubscribersCount
    {
        get => _subscribersCount;
        set
        {
            if (_subscribersCount != value)
            {
                _subscribersCount = value;
                OnPropertyChanged();
            }
        }
    }


    public int? Roleid { get; set; }

    //public virtual ICollection<Goal> GoalAdmins { get; set; } = new List<Goal>();

    public virtual ICollection<Goal> GoalCreators { get; set; } = new List<Goal>();

    //public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();

    //public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<News> News { get; set; } = new List<News>();

    public virtual ICollection<PersonalTask> PersonalTasks { get; set; } = new List<PersonalTask>();

    public virtual Point? Point { get; set; }

    public virtual ICollection<PointsHistory> PointsHistories { get; set; } = new List<PointsHistory>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Role? Role { get; set; }

    //public virtual ICollection<Scoretransaction> Scoretransactions { get; set; } = new List<Scoretransaction>();

    public virtual ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

    public virtual ICollection<Subscription> SubscriptionSubscribedTos { get; set; } = new List<Subscription>();

    public virtual ICollection<Subscription> SubscriptionSubscribers { get; set; } = new List<Subscription>();

    //public virtual ICollection<TaskExecutionHistory> TaskExecutionHistories { get; set; } = new List<TaskExecutionHistory>();

    public virtual ICollection<UserGiftCard> UserGiftCards { get; set; } = new List<UserGiftCard>();

    public virtual ICollection<Usersetting> Usersettings { get; set; } = new List<Usersetting>();

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>(); // Добавлено


    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
