using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.DataCheckers;
using Diploma_Ishchenko.Services.PostgresServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Diploma_Ishchenko;

public partial class LoginWindow : Window, INotifyPropertyChanged
{
    private Avalonia.Point _mousePosition;
    private bool _isDragging;
    public bool IsRegister { get; set; } = false;

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
    private async void LoginAccount_ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowErrorMessage("Поле email не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                ShowErrorMessage("Поле пароля не может быть пустым");
                return;
            }

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(EmailTextBox.Text))
            {
                ShowErrorMessage("Неверный формат email");
                return;
            }

            var user = UserService.GetUserByEmail(EmailTextBox.Text.Trim().ToLower());
            if (user == null)
            {
                ShowErrorMessage("Пользователь с таким email не найден");
                return;
            }

            if (BannedUsersService.FindBannedUser(user))
            {
                var bannedUser = BannedUsersService.GetBannedUser(user);
                var message = $"Доступ в приложение заблокирован до {bannedUser.BanEnddate:dd.MM.yyyy HH:mm} " +
                             $"по причине: {bannedUser.Reason}";

                await new MessageBox(message, false).ShowDialog(this);
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(PasswordTextBox.Text, user.PasswordHash))
            {
                ShowErrorMessage("Неверный пароль");
                return;
            }

            // Успешная авторизация
            AuthorizedUser.User = user;
            LoadUserTheme(user.Id);

            new ContentContainer().Show();
            this.Close();
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Login error: {ex.Message}");
            ShowErrorMessage("Произошла ошибка при входе. Попробуйте позже.");
        }
    }

    private void LoadUserTheme(int userId)
    {
        using (var context = new PlanItContext())
        {
            var userSettings = context.Usersettings
                .Include(us => us.Colortheme)
                .FirstOrDefault(us => us.Userid == userId);

            if (userSettings?.Colortheme != null)
            {
                // Применяем тему и сохраняем в локальный файл
                var app = (App)Application.Current;
                app.SetTheme(userSettings.Colortheme.Name);
            }
        }
    }

    private async void GoogleAuth_ButtonClick(object sender, RoutedEventArgs e)
    {
        var authenticator = new GoogleAuthenticator();
        var userInfo = await authenticator.AuthenticateAsync();

        var existingUser = UserService.GetUserByEmail(userInfo.Email);
        if (existingUser == null)
        {
            var user = await UserService.AddUserByGoogle(userInfo);

            await UserService.AddUser(user);

            var setNickname = new SetNicknameAndPassword(user);
            var result = await setNickname.ShowDialog<bool>(this);
            if (result)
            {
                await UserService.AddBonusWalletToUser(user);
                AuthorizedUser.User = user;

                // Применяем локальную тему (либо из файла, либо дефолтную)
                var app = (App)Application.Current;
                app.SetTheme(app.LoadLocalSettings().Theme);

                // Создаем настройки с темой по умолчанию
                await AddDefaultUserSettings(user.Id);

                ContentContainer container = new ContentContainer();
                container.Show();
                this.Close();
            }
        }
        else
        {
            var ifUserBanned = BannedUsersService.FindBannedUser(existingUser);
            if (ifUserBanned)
            {
                var bannedUser = BannedUsersService.GetBannedUser(existingUser);
                await new MessageBox($"Доступ в приложение заблокирован до {bannedUser.BanEnddate} по причине {bannedUser.Reason}!", false).ShowDialog(this);
                return;
            }

            AuthorizedUser.User = existingUser;

            ContentContainer container = new ContentContainer();
            container.Show();
            this.Close();
        }
    }

    private async void RecoverPassword_ButtonClick(object sender, RoutedEventArgs e)
    {
        ForgetPassword forgetPassword = new ForgetPassword();
        await forgetPassword.ShowDialog(this);
    }

    private async void RegisterAccount_ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(EmailTextBox.Text) && !string.IsNullOrEmpty(PasswordTextBox.Text) &&
                !string.IsNullOrEmpty(NicknameTextBox.Text) && !string.IsNullOrEmpty(UsernameTextBox.Text) &&
                !string.IsNullOrEmpty(RepeatPasswordTextBox.Text))
            {
                if (!PasswordTextBox.Text.Equals(RepeatPasswordTextBox.Text))
                {
                    ShowErrorMessage(message: "Пароли не совпадают");
                    return;
                }

                //добавить тут также проверку на ввод достаточно сложного пароля
                if (!DataCheckerClass.IsValidPassword(PasswordTextBox.Text))
                {
                    ShowErrorMessage(message: "Пароль должен содержать цифры, буквы верхнего и нижнего регистра");
                    return;
                }

                var user = UserService.GetUserByEmail(EmailTextBox.Text);
                if (user != null)
                {
                    ShowErrorMessage(message: "Пользователь с таким Email существует");
                    return;
                }

                var userByNickname = UserService.GetUserByNickname(NicknameTextBox.Text);
                if (userByNickname != null)
                {
                    ShowErrorMessage(message: "Такой никнейм уже существует");
                    return;
                }

                ErrorMessage.IsVisible = false;
                var newUser = new User
                {
                    Username = UsernameTextBox.Text,
                    Email = EmailTextBox.Text,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordTextBox.Text),
                    Nickname = NicknameTextBox.Text,
                    CreatedAt = DateTime.Now,
                    IsAdmin = false,
                    Userphoto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars", "default", "no-profile-picture.png")
                };

                await UserService.AddUser(newUser); // Асинхронное добавление
                await UserService.AddBonusWalletToUser(newUser);

                AuthorizedUser.User = newUser;

                // Добавляем настройки с темой по умолчанию
                await AddDefaultUserSettings(newUser.Id);

                // Применяем локальную тему (из файла или дефолтную)
                var app = (App)Application.Current;
                app.SetTheme(app.LoadLocalSettings().Theme);

                ContentContainer container = new ContentContainer();
                container.Show();
                this.Close();
            }
            else
            {
                ShowErrorMessage(message: "Не все поля заполнены");
                return;
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Ошибка регистрации: {ex.Message}");
        }
    }

    private async Task AddDefaultUserSettings(int userId)
    {
        using (var context = new PlanItContext())
        {
            context.Usersettings.Add(
            
            new Usersetting
            {
                Id = context.Usersettings.Any() ? context.Usersettings.Max(u => u.Id) + 1 : 1,
                Userid = userId,
                Colorthemeid = 1, // ID базовой темы из БД
                Isshownotifications = false
            });
            await context.SaveChangesAsync();
        }
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage.IsVisible = true;
        ErrorMessage.Text = message;
    }

    private void ChooseLogin_ButtonClick(object sender, RoutedEventArgs e)
    {
        IsRegister = false;
        OnPropertyChanged(nameof(IsRegister));
    }

    private void ChooseRegister_ButtonClick(object sender, RoutedEventArgs e)
    {
        IsRegister = true;
        OnPropertyChanged(nameof(IsRegister));
    }

    private void Exit_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (this.VisualRoot is Window mainWindow)
        {
            mainWindow.Close(); // Закрываем главное окно
        }
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            _mousePosition = point.Position;
            _isDragging = true;
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPosition = e.GetCurrentPoint(this).Position;
            var delta = currentPosition - _mousePosition;
            this.Position = new PixelPoint(this.Position.X + (int)delta.X, this.Position.Y + (int)delta.Y);
        }
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}