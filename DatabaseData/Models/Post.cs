using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Post : INotifyPropertyChanged
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TaskId { get; set; }

    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

    public virtual PersonalTask? Task { get; set; }

    public virtual User? User { get; set; }

    [NotMapped]
    public bool IsCurrentUser => UserId == AuthorizedUser.User.Id; // CurrentUserId - текущий пользователь

    //public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    private ObservableCollection<PostComment> _postComments = new ObservableCollection<PostComment>();

    [NotMapped]
    public virtual ObservableCollection<PostComment> PostComments
    {
        get => _postComments;
        set
        {
            if (_postComments != value)
            {
                _postComments = value;
                OnPropertyChanged();
            }
        }
    }


    [NotMapped] private string _commentMessage;
    [NotMapped] private string _errorMessage;
    [NotMapped] private bool _isErrorVisible;
    [NotMapped] private bool _isCommentsVisible;

    [NotMapped]
    public string CommentMessage
    {
        get => _commentMessage;
        set { _commentMessage = value; OnPropertyChanged(); }
    }

    [NotMapped]
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    [NotMapped]
    public bool IsErrorVisible
    {
        get => _isErrorVisible;
        set { _isErrorVisible = value; OnPropertyChanged(); }
    }

    [NotMapped]
    public bool IsCommentsVisible
    {
        get => _isCommentsVisible;
        set
        {
            _isCommentsVisible = value;
            OnPropertyChanged();
            //OnPropertyChanged(nameof(IsCommentsVisible)); // Для инверсии в верстке
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
