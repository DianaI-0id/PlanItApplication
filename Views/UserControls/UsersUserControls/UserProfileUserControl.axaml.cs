using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.DataCheckers;
using Diploma_Ishchenko.Services.PostgresServices;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using System.Diagnostics;
using Diploma_Ishchenko.ImageServices;
using Supabase.Gotrue;
using Diploma_Ishchenko.DatabaseData.Context;
using Microsoft.EntityFrameworkCore;

namespace Diploma_Ishchenko;

public partial class UserProfileUserControl : UserControl, INotifyPropertyChanged
{

    private readonly Dictionary<string, string> _themeNameMap = new()
    {
        //список тем
        { "LightBlue Theme", "DefaultTheme" },
        { "LightRed Theme", "DefaultRedTheme" },
        { "DarkViolet Theme", "DarkTheme" }
    };

    public List<string> ThemeDisplayNames => _themeNameMap.Keys.ToList();

    public bool IsFieldsEnabled { get; set; } = false; //доступность редактирования закрыта по умолчанию
    public bool IsSubscriptionActive { get; set; } //проверяем подписку для изменения видимости полей
    public DatabaseData.Models.User User { get; set; }
    public bool IsAdmin { get; set; } //видимость полей в зависимости от статуса

    private int _currentUserId; // Получаем из контекста или параметров

    private Bitmap _avatarBitmap;
    public Bitmap AvatarBitmap
    {
        get => _avatarBitmap;
        set
        {
            _avatarBitmap = value;
            OnPropertyChanged(nameof(AvatarBitmap));
        }
    }

    public bool IsUserHasTelegramId { get; set; } //если у пользователя отсутвует тг айди и он без подписки

    public bool IsUserSubscriptedAndHasTelegramId { get; set; } //если пользователь с подпиской и с тг айди

    public UserProfileUserControl()
    {
        InitializeComponent();

        User = AuthorizedUser.User;

        IsSubscriptionActive = (bool)User.HasActiveSubscription ? true : false;
        OnPropertyChanged(nameof(IsSubscriptionActive));

        IsAdmin = User.Roleid == 2 ? true : false;
        OnPropertyChanged(nameof(IsAdmin));

        //загружаем данные о подписках и подписчиках
        User.SubscriptionsCount = UserService.GetSubscriptionsCount(User.Id);
        User.SubscribersCount = UserService.GetSubscribersCount(User.Id);

        // Инициализация аватарки при загрузке
        LoadUserImage();

        UpdateTelegramInfoVisibility();

        DataContext = this;
    }

    private void UpdateTelegramInfoVisibility()
    {
        if (User.TelegramId != null)
        {
            IsUserHasTelegramId = true;
            OnPropertyChanged(nameof(IsUserHasTelegramId));

            //если в боте отказаться от использования бота, то телеграмid из базы данных должен очиститься
        }

        if (User.TelegramId != null && User.HasActiveSubscription == true)
        {
            IsUserSubscriptedAndHasTelegramId = true;
            OnPropertyChanged(nameof(IsUserSubscriptedAndHasTelegramId));
        }
    }

    private async void ChangeAvatar_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contenContainer = this.GetVisualRoot() as ContentContainer;
        var result = await ImageService.ProcessImageUpload(AuthorizedUser.User.Id, contenContainer);

        if (result?.Bitmap != null && result.FilePath != null)
        {
            AvatarBitmap = result.Bitmap;

            // Преобразуем абсолютный путь в относительный
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = Path.GetRelativePath(baseDir, result.FilePath).Replace("\\", "/");

            // Обновляем путь к аватару в базе данных
            var user = UserService.GetUserByEmail(AuthorizedUser.User.Email);
            if (user != null)
            {
                user.Userphoto = relativePath;  // Сохраняем относительный путь
                await UserService.UpdateUser(user); // Метод обновления пользователя в БД
            }

            await new MessageBox("Изображение успешно изменено!", false).ShowDialog(contenContainer);
        }
    }

    private void LoadUserImage()
    {
        AvatarBitmap = ImageService.LoadUserImage(AuthorizedUser.User.Id);
    }

    private void EnableEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        IsFieldsEnabled = !IsFieldsEnabled;
        OnPropertyChanged(nameof(IsFieldsEnabled));
    }

    //редактирование основных данных пользователя
    private async void ApplyEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        //проверка на заполненность обязательных полей
        if (!string.IsNullOrEmpty(UsernameTextBox.Text) && !string.IsNullOrEmpty(NicknameTextBox.Text))
        {
            if (CheckIfNicknameUnique(NicknameTextBox.Text))
            {
                User.Nickname = NicknameTextBox.Text;
                User.Biography = string.IsNullOrEmpty(BiographyTextBox.Text) ? "Информация отсутствует" : BiographyTextBox.Text;
                User.Username = UsernameTextBox.Text;

                UserService.UpdateUser(User);

                UsernameTextBox.Text = User.Username;
                BiographyTextBox.Text = User.Biography;
                NicknameTextBox.Text = User.Nickname;

                IsFieldsEnabled = !IsFieldsEnabled;
                OnPropertyChanged(nameof(IsFieldsEnabled));

                var contentContainer = this.GetVisualRoot() as ContentContainer;

                MessageBox messageBox = new MessageBox("Данные успешно обновлены!", false);
                await messageBox.ShowDialog(contentContainer);
            }
            else
            {
                ShowErrorMessage(message: "Пользователь с таким Nickname уже существует", UserDataErrorMessage);
            }
        }
        else
        {
            ShowErrorMessage(message: "Не все поля заполнены", UserDataErrorMessage);
        }
    }

    private bool CheckIfNicknameUnique(string nickname) //проверяем существует ли уже введенный никнейм в базе данных 
    {
        var nicknameExists = UserService.GetUserByNickname(nickname);

        if (nicknameExists != null && nicknameExists.Id != AuthorizedUser.User.Id)
        {
            return false;
        }

        return true;
    }

    private async void ChangeUserTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem is string displayName)
        {
            if (!_themeNameMap.TryGetValue(displayName, out var dbThemeName))
                return; // Не нашли соответствие - выход

            using (var context = new PlanItContext())
            {
                var userSetting = context.Usersettings.Include(t => t.Colortheme)
                    .FirstOrDefault(u => u.Userid == AuthorizedUser.User.Id);

                var theme = context.Colorthemes.FirstOrDefault(t => t.Name == dbThemeName);
                if (theme == null)
                    return; // Тема не найдена в БД

                userSetting.Colorthemeid = theme.Id;
                context.Usersettings.Update(userSetting);
                context.SaveChanges();

                // Обновляем локальную тему
                var app = (App)Application.Current;
                app.SetTheme(theme.Name);
            }
        }
    }


    private async void ChangePassword_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PasswordTextBox.Text) && !string.IsNullOrEmpty(RepeatPasswordTextBox.Text))
        {
            if (PasswordTextBox.Text.Equals(RepeatPasswordTextBox.Text))
            {
                var isPasswordValid = DataCheckerClass.IsValidPassword(PasswordTextBox.Text);
                if (isPasswordValid)
                {
                    User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordTextBox.Text);
                    UserService.UpdateUser(User);

                    var contentContainer = this.GetVisualRoot() as ContentContainer;

                    MessageBox messageBox = new MessageBox("Данные успешно обновлены!", false);
                    await messageBox.ShowDialog(contentContainer);

                    PasswordTextBox.Clear();
                    RepeatPasswordTextBox.Clear();

                    PasswordErrorMessage.IsVisible = false;
                }
                else
                {
                    ShowErrorMessage(message: "Пароль должен содержать буквы верхнего и нижнего регистров, цифру и спец. символ", PasswordErrorMessage);
                }
            }
            else
            {
                ShowErrorMessage(message: "Пароли не совпадают", PasswordErrorMessage);
            }
        }
        else
        {
            ShowErrorMessage(message: "Поля не заполнены", PasswordErrorMessage);
        }
    }

    //закрыть доступ к редактированию, скрыть кнопки и в поля ввода вернуть данные из базы
    private void RemoveChanges_ButtonClick(object sender, RoutedEventArgs e)
    {
        // Получаем свежие данные из базы
        var originalUser = UserService.GetUserByEmail(AuthorizedUser.User.Email);

        UsernameTextBox.Text = originalUser.Username;
        BiographyTextBox.Text = originalUser.Biography;
        NicknameTextBox.Text = originalUser.Nickname;

        IsFieldsEnabled = !IsFieldsEnabled;
        OnPropertyChanged(nameof(IsFieldsEnabled));
    }

    private void ShowSubscriptionsInfo_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        var subscribe = new UserSubscriptionsUserControl();
        contentContainer.ContentControlContainer.Content = subscribe;
    }

    private void ShowErrorMessage(string message, TextBlock errorTextBlock)
    {
        errorTextBlock.IsVisible = true;
        errorTextBlock.Text = message;
    }

    private void Subscribe_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        var subscribe = new SubscribeUserControl(this);
        contentContainer.ContentControlContainer.Content = subscribe;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ImageUploadResult //мини-класс для коррекного обновления данных о пути фото
{
    public Bitmap? Bitmap { get; set; }
    public string? FilePath { get; set; }
}
