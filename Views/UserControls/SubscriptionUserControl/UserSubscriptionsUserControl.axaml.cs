using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Diploma_Ishchenko;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Diploma_Ishchenko
{
    public partial class UserSubscriptionsUserControl : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _searchTimer;
        private string _searchText = string.Empty;


        private ObservableCollection<User> subscriptions = new ObservableCollection<User>();
        private ObservableCollection<User> subscribers = new ObservableCollection<User>();

        private ObservableCollection<User> items = new ObservableCollection<User>();
        public ObservableCollection<User> Items
        {
            get => items;
            private set
            {
                if (items != value)
                {
                    items = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isShowingSubscriptions = true;
        public bool IsShowingSubscriptions
        {
            get => isShowingSubscriptions;
            private set
            {
                if (isShowingSubscriptions != value)
                {
                    isShowingSubscriptions = value;
                    OnPropertyChanged();
                    UpdateUI();
                }
            }
        }

        public bool IsNoSubscriptionsVisible => IsShowingSubscriptions && Items.Count == 0;
        public bool IsNoSubscribersVisible => !IsShowingSubscriptions && Items.Count == 0;

        public bool IsSubscriptionsButtonEnabled => !IsShowingSubscriptions;
        public bool IsSubscribersButtonEnabled => IsShowingSubscriptions;

        public UserSubscriptionsUserControl()
        {
            InitializeComponent();

            DataContext = this;

            _ = LoadDataAsync();

            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(200);
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private async Task LoadDataAsync()
        {
            if (AuthorizedUser.User == null)
                return;

            int currentUserId = AuthorizedUser.User.Id;

            var subs = await SubscriptionService.GetSubscriptionsAsync(currentUserId);
            var subsr = await SubscriptionService.GetSubscribersAsync(currentUserId);

            subscriptions.Clear();
            foreach (var s in subs)
                subscriptions.Add(s);

            subscribers.Clear();
            foreach (var s in subsr)
                subscribers.Add(s);

            UpdateUI();
        }

        private void SearchUser_KeyUp(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            _searchText = textBox.Text?.ToLower() ?? string.Empty;

            // Сбрасываем таймер
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void ApplyFilter(string filter)
        {
            IEnumerable<User> source = IsShowingSubscriptions ? subscriptions : subscribers;

            if (string.IsNullOrWhiteSpace(filter))
            {
                Items = new ObservableCollection<User>(source);
            }
            else
            {
                var filtered = source.Where(u =>
                    (u.Nickname?.ToLower().Contains(filter) ?? false) ||
                    (u.Username?.ToLower().Contains(filter) ?? false));

                Items = new ObservableCollection<User>(filtered);
            }

            OnPropertyChanged(nameof(IsNoSubscriptionsVisible));
            OnPropertyChanged(nameof(IsNoSubscribersVisible));
        }

        private void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();

            ApplyFilter(_searchText);
        }

        private void ShowUserPosts_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border panel && panel.DataContext is User user)
            {
                var contentContainer = this.GetVisualRoot() as ContentContainer;

                //НУЖНО ТАКЖЕ ПЕРЕДАВАТЬ ОКНО ДЛЯ ВОЗВРАЩЕНИЯ
                contentContainer.ContentControlContainer.Content = new MyPostsUserControl(user, this);
            }
        }

        private void BtnSubscriptions_Click(object? sender, RoutedEventArgs e)
        {
            ShowSubscriptions();
        }

        private void BtnSubscribers_Click(object? sender, RoutedEventArgs e)
        {
            ShowSubscribers();
        }

        private void ShowSubscriptions()
        {
            IsShowingSubscriptions = true;
            ApplyFilter(_searchText);
        }

        private void ShowSubscribers()
        {
            IsShowingSubscriptions = false;
            ApplyFilter(_searchText);
        }

        private void UpdateUI()
        {
            Items.Clear();

            if (IsShowingSubscriptions)
            {
                foreach (var sub in subscriptions)
                {
                    if (sub.AvatarImage == null)
                        sub.AvatarImage = ImageService.LoadUserImage(sub.Id, sub.Userphoto);

                    sub.IsSubscription = true; // это ваша подписка
                    Items.Add(sub);
                }
            }
            else
            {
                foreach (var sub in subscribers)
                {
                    if (sub.AvatarImage == null)
                        sub.AvatarImage = ImageService.LoadUserImage(sub.Id, sub.Userphoto);

                    sub.IsSubscription = false; // это подписчик
                    Items.Add(sub);
                }
            }

            OnPropertyChanged(nameof(IsNoSubscriptionsVisible));
            OnPropertyChanged(nameof(IsNoSubscribersVisible));
            OnPropertyChanged(nameof(IsSubscriptionsButtonEnabled));
            OnPropertyChanged(nameof(IsSubscribersButtonEnabled));
        }

        private async void DropSubscription_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                var msBox = new MessageBox($"Вы уверены, что хотите отписаться от {user.Nickname}?", true);
                var result = await msBox.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);

                if (result)
                {
                    await SubscriptionService.DropSubscription(AuthorizedUser.User.Id, user.Id);

                    subscriptions.Remove(user);

                    UpdateUI();

                    var successMessage = new MessageBox($"Вы успешно отписались от {user.Nickname}!", false);
                    await successMessage.ShowDialog<bool>(this.GetVisualRoot() as ContentContainer);
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
            
    }
}
