using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diploma_Ishchenko
{
    public partial class CommunityUserControl : UserControl, INotifyPropertyChanged
    {
        private string _postTitleSearch = string.Empty; //строка для поиска постов по названию
        private DispatcherTimer _searchTimer; //для обновления постов после поиска с заданной частотой
        public ObservableCollection<Post> PostsCollection { get; set; } = new ObservableCollection<Post>();

        private string _sortString = string.Empty;

        public CommunityUserControl()
        {
            InitializeComponent();

            // Инициализация таймера для поиска с задержкой
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _searchTimer.Tick += OnSearchTimerTick;

            LoadPosts();
            DataContext = this;
        }

        private void LoadPosts()
        {
            var posts = PostsService.LoadPosts();
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
                        comment.IsCurrentUserAdmin = AuthorizedUser.User.Roleid == 2;
                    }

                    bool isCurrentUser = post.User.Id == AuthorizedUser.User.Id;
                    bool isSubscribed = SubscriptionService.IsSubscribed(AuthorizedUser.User.Id, post.User.Id);

                    post.User.IsCurrentUser = isCurrentUser;
                    post.User.IsNotCurrentSubscriptedUser = !isCurrentUser && !isSubscribed;

                    post.User.IsCurrentUserNotAdmin = AuthorizedUser.User.Roleid != 2;
                    post.User.IsCurrentUserAdmin = AuthorizedUser.User.Roleid == 2;

                    PostsCollection.Add(post);
                }
            }
        }

        private void LoadPostsBySearchString()
        {
            var allPosts = PostsService.LoadPosts();

            if (!string.IsNullOrEmpty(_postTitleSearch))
            {
                var filteredPosts = allPosts.Where(p =>
                    p.Title?.ToLower().Contains(_postTitleSearch) == true).ToList();

                filteredPosts = SortingMethod(filteredPosts);
                UpdatePostsCollection(filteredPosts);
            }
            else
            {
                allPosts = SortingMethod(allPosts);
                UpdatePostsCollection(allPosts);
            }
        }

        private List<Post> SortingMethod(List<Post> filteredPosts)
        {
            //сортировка
            switch (_sortString)
            {
                case "Сначала новые":
                    filteredPosts = filteredPosts.OrderByDescending(p => p.CreatedAt).ToList();
                    break;
                case "Сначала старые":
                    filteredPosts = filteredPosts.OrderBy(p => p.CreatedAt).ToList();
                    break;
            }

            return filteredPosts;
        }

        private void SearchPosts_KeyUp(object sender, KeyEventArgs e)
        {
            // Сбрасываем таймер при каждом нажатии клавиши
            _searchTimer.Stop();

            if (!string.IsNullOrEmpty(searchTextBox.Text))
            {
                _postTitleSearch = searchTextBox.Text.ToLower();
            }
            else
            {
                _postTitleSearch = string.Empty;
            }

            // Запускаем таймер заново
            _searchTimer.Start();
        }

        private void OnSearchTimerTick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadPostsBySearchString();
        }

        private void ImageListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                // Если нет картинок — скрываем ListBox
                listBox.IsVisible = listBox.Items.Count > 0;
            }
        }

        private void PublishPost_ButtonClick(object sender, RoutedEventArgs e)
        {
            if ((bool)!AuthorizedUser.User.HasActiveSubscription)
            {
                ShowErrorMessageBox(message: "Доступно только по подписке");
                return;
            }

            var contentContainer = this.GetVisualRoot() as ContentContainer;
            contentContainer.ContentControlContainer.Content = new PublishPostUserControl();
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

        private async void DeleteSelectedComment_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PostComment comment)
            {
                // Удаляем комментарий из базы/сервиса
                await PostsService.DeleteComment(comment.Id);

                // Находим пост, которому принадлежит комментарий
                var post = PostsCollection.FirstOrDefault(p => p.Id == comment.PostId);
                if (post != null)
                {
                    // Удаляем комментарий из коллекции комментариев поста
                    post.PostComments.Remove(comment);
                }
            }
        }

        private async void SendComment_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                if (!(bool)AuthorizedUser.User.HasActiveSubscription)
                {
                    ShowErrorMessageBox(message: "Доступно только по подписке");
                    post.CommentMessage = string.Empty;
                    return;
                }

                if (string.IsNullOrEmpty(post.CommentMessage))
                {
                    post.IsErrorVisible = true;
                    post.ErrorMessage = "Комментарий не может быть пустым";
                    return;
                }

                var newComment = new PostComment
                {
                    PostId = post.Id,
                    UserId = AuthorizedUser.User.Id,
                    Content = post.CommentMessage,
                    CreatedAt = DateTime.UtcNow
                };

                await PostCommentsService.AddComment(newComment);

                // Загружаем обновленные комментарии
                var updatedComments = PostsService.LoadComments(post.Id).ToList();

                // Обновляем свойства комментариев (User, AvatarImage, IsCurrentUserComment)
                foreach (var comment in updatedComments)
                {
                    comment.User = UserService.GetUserById(comment.UserId);
                    comment.User.AvatarImage = ImageService.LoadUserImage(comment.UserId);
                    comment.IsCurrentUserComment = comment.UserId == AuthorizedUser.User.Id;
                }

                // Обновляем коллекцию комментариев поста
                post.PostComments = new ObservableCollection<PostComment>(updatedComments);

                // Очищаем поле ввода и ошибки
                post.CommentMessage = string.Empty;
                post.IsErrorVisible = false;
            }
        }

        private async void ShowCommunityRules_ButtonClick(object sender, RoutedEventArgs e)
        {
            var container = this.GetVisualRoot() as ContentContainer;
            var rulesWindow = new CommunityRules();

            await rulesWindow.ShowDialog(container);
        }

        private async void SubscribeToUser_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                try
                {
                    await SubscriptionService.SubscribeToUser(AuthorizedUser.User, post.User);

                    // Обновляем свойства видимости кнопок
                    post.User.IsNotCurrentSubscriptedUser = false; // Теперь подписка есть - кнопка "Подписаться" скрывается
                    //post.User.IsSubscribedUser = true;             // Кнопка "Отписаться" появляется

                    await new MessageBox($"Подписка успешно оформлена!", false).ShowDialog(this.GetVisualRoot() as ContentContainer);

                }
                catch (Exception ex)
                {
                    await new MessageBox($"{ex.Message}", false).ShowDialog(this.GetVisualRoot() as ContentContainer);
                }
            }
        }

        private async void DeleteSelectedPostByAdmin_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                var messageBox = new MessageBox("Это действие нельзя будет отменить, вы уверены?", true);
                var result = await messageBox.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);

                if (result)
                {
                    PostsCollection.Remove(post);
                    await PostsService.DeletePost(post);

                    await new MessageBox("Пост успешно удален", false).ShowDialog(this.GetVisualRoot() as ContentContainer);
                }
            }
        }

        private async void DropSubscribeToUser_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                try
                {
                    await SubscriptionService.DropUserSubscribe(AuthorizedUser.User, post.User);

                    // Обновляем свойства видимости кнопок
                    post.User.IsNotCurrentSubscriptedUser = true; // Подписки нет - кнопка "Подписаться" появляется
                    //post.User.IsSubscribedUser = false;             // Кнопка "Отписаться" скрывается

                    await new MessageBox($"Подписка успешно отменена!", false).ShowDialog(this.GetVisualRoot() as ContentContainer);
                }
                catch (Exception ex)
                {
                    await new MessageBox($"{ex.Message}", false).ShowDialog(this.GetVisualRoot() as ContentContainer);
                }
            }
        }

        private void SortCommunityPosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                _sortString = item.Content.ToString();
                LoadPostsBySearchString();
            }
        }

        private void ShowErrorMessageBox(string message)
        {
            var messageBox = new MessageBox(message, false);
            messageBox.ShowDialog(this.GetVisualRoot() as ContentContainer);
        }

        private void ShowUserInfo_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border panel && panel.DataContext is Post post)
            {
                var contentContainer = this.GetVisualRoot() as ContentContainer;

                contentContainer.ContentControlContainer.Content = new MyPostsUserControl(post.User, this);
            }
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
}