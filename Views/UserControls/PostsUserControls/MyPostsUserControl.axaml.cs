using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Diploma_Ishchenko;

public partial class MyPostsUserControl : UserControl, INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private string sortText;

    private DispatcherTimer _searchDebounceTimer;

    public User User { get; set; }

    public bool IsAuthorizedUser => User.Id == AuthorizedUser.User.Id; //если мы открываем для другого пользователя, то некоторые поля нужно скрыть
    public bool IsCurrentUserAdmin => AuthorizedUser.User.Roleid == 2;

    public ObservableCollection<Post> PostsCollection { get; set; } = new ObservableCollection<Post>();

    private UserControl _baseUserControl;
    public MyPostsUserControl()
    {
        InitializeComponent();
        User = AuthorizedUser.User;
        LoadPosts();     
        DataContext = this;
    }

    public MyPostsUserControl(User user, UserControl baseUserControl)
    {
        InitializeComponent();
        User = user;

        _baseUserControl = baseUserControl;

        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200) // Задержка 200 мс
        };
        _searchDebounceTimer.Tick += OnSearchDebounceTimerTick;

        LoadPosts();
        DataContext = this;
    }

    private void LoadPosts()
    {
        var posts = PostsService.LoadPosts();
        posts = posts.Where(u => u.UserId == User.Id).ToList();
        UpdatePostsCollection(posts);
    }

    private void UpdatePostsCollection(IEnumerable<Post> posts)
    {
        PostsCollection.Clear();

        foreach (var post in posts)
        {
            if (post.User != null)
            {
                post.User.AvatarImage = ImageService.LoadUserImage(post.User.Id);

                foreach (var image in post.PostImages)
                {
                    image.ImageBitmap = PostImageService.LoadBitmapFromImagePath(image.ImagePath);
                }

                post.PostComments = new ObservableCollection<PostComment>(
                    PostsService.LoadComments(post.Id)
                );

                foreach (var comment in post.PostComments)
                {
                    comment.User = UserService.GetUserById(comment.UserId);
                    comment.User.AvatarImage = ImageService.LoadUserImage(comment.UserId);
                    comment.IsCurrentUserComment = comment.UserId == AuthorizedUser.User.Id;
                }

                bool isCurrentUser = post.User.Id == AuthorizedUser.User.Id;
                bool isSubscribed = SubscriptionService.IsSubscribed(AuthorizedUser.User.Id, post.User.Id);

                post.User.IsCurrentUser = isCurrentUser;
                post.User.IsNotCurrentSubscriptedUser = !isCurrentUser && !isSubscribed;

                PostsCollection.Add(post);
            }
        }
    }

    private async void BanUser_ButtonClick(object sender, RoutedEventArgs e)
    {
        //тут нужно вывести предупреждение, и если ДА, то предлагаем вариант длительности + срок бана

        var bannedUser = BannedUsersService.FindBannedUser(User);
        if (bannedUser)
        {
            var message = new MessageBox("Пользователю уже заблокирован доступ!", false);
            await message.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);
            return;
        }

        var msBox = new MessageBox("Если пользователь нарушает правила сообщества, можно заблокировать ему доступ в приложение. Вы уверены?", true);
        var result = await msBox.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);

        if (result)
        {
            //тут вызываем окошко и ждем подтверждения
            var confirmWindow = new BanUserWindow(User);
            await confirmWindow.ShowDialog(this.GetVisualRoot() as ContentContainer); //ожидаем завершения операции
        }
    }

    private void ImageListBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            // Если нет картинок — скрываем ListBox
            listBox.IsVisible = listBox.Items.Count > 0;
        }
    }

    private void OnSearchDebounceTimerTick(object sender, EventArgs e)
    {
        _searchDebounceTimer.Stop();
        SearchSortPosts();
    }

    private void SearchSortPosts()
    {
        using (var context = new PlanItContext())
        {
            var posts = PostsService.LoadPosts().Where(p => p.UserId == User.Id).ToList();

            if (!string.IsNullOrEmpty(_searchText))
            {
                string[] searchwords = _searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                posts = posts.Where(post =>
                    post.Title != null &&
                    searchwords.All(word =>
                        post.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            // Сортировка
            switch (sortText)
            {
                case "Сначала новые":
                    posts = posts.OrderByDescending(u => u.CreatedAt).ToList();
                    break;
                case "Сначала старые":
                    posts = posts.OrderBy(u => u.CreatedAt).ToList();
                    break;
            }

            // Обновление коллекции
            PostsCollection.Clear();
            foreach (var post in posts)
            {
                post.User.AvatarImage = ImageService.LoadUserImage(post.User.Id);

                foreach (var image in post.PostImages)
                {
                    image.ImageBitmap = PostImageService.LoadBitmapFromImagePath(image.ImagePath);
                }

                PostsCollection.Add(post);
            }
        }
    }

    private void SearchPosts_KeyUp(object sender, KeyEventArgs e)
    {
        _searchText = SearchText.Text?.Trim().ToLower() ?? string.Empty;
        _searchDebounceTimer.Stop(); // Сбрасываем таймер при каждом нажатии
        _searchDebounceTimer.Start(); // Перезапускаем таймер
    }

    private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
        {
            sortText = item.Content.ToString();
            SearchSortPosts();
        }
    }

    private void ShowComments_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Post post)
        {
            post.IsCommentsVisible = true;
        }
    }

    private void HideComments_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Post post)
        {
            post.IsCommentsVisible = false;
        }
    }

    private void Back_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = _baseUserControl;
    }

    private void Image_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Image image && image.Source is Bitmap bitmap && !ImageOverlay.IsVisible)
        {
            EnlargedImage.Source = bitmap;
            ImageOverlay.IsVisible = true;
            e.Handled = true;
        }
    }

    private void ImageOverlay_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Закрываем оверлей при клике в любом месте
        ImageOverlay.IsVisible = false;
        e.Handled = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ImageOverlay.IsVisible = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}