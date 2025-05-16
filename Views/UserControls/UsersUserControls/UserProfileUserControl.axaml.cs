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
        //������ ���
        { "LightBlue Theme", "DefaultTheme" },
        { "LightRed Theme", "DefaultRedTheme" },
        { "DarkViolet Theme", "DarkTheme" }
    };

    public List<string> ThemeDisplayNames => _themeNameMap.Keys.ToList();

    public bool IsFieldsEnabled { get; set; } = false; //����������� �������������� ������� �� ���������
    public bool IsSubscriptionActive { get; set; } //��������� �������� ��� ��������� ��������� �����
    public DatabaseData.Models.User User { get; set; }
    public bool IsAdmin { get; set; } //��������� ����� � ����������� �� �������

    private int _currentUserId; // �������� �� ��������� ��� ����������

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

    public bool IsUserHasTelegramId { get; set; } //���� � ������������ ��������� �� ���� � �� ��� ��������

    public bool IsUserSubscriptedAndHasTelegramId { get; set; } //���� ������������ � ��������� � � �� ����

    public UserProfileUserControl()
    {
        InitializeComponent();

        User = AuthorizedUser.User;

        IsSubscriptionActive = (bool)User.HasActiveSubscription ? true : false;
        OnPropertyChanged(nameof(IsSubscriptionActive));

        IsAdmin = User.Roleid == 2 ? true : false;
        OnPropertyChanged(nameof(IsAdmin));

        //��������� ������ � ��������� � �����������
        User.SubscriptionsCount = UserService.GetSubscriptionsCount(User.Id);
        User.SubscribersCount = UserService.GetSubscribersCount(User.Id);

        // ������������� �������� ��� ��������
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

            //���� � ���� ���������� �� ������������� ����, �� ��������id �� ���� ������ ������ ����������
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

            // ����������� ���������� ���� � �������������
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = Path.GetRelativePath(baseDir, result.FilePath).Replace("\\", "/");

            // ��������� ���� � ������� � ���� ������
            var user = UserService.GetUserByEmail(AuthorizedUser.User.Email);
            if (user != null)
            {
                user.Userphoto = relativePath;  // ��������� ������������� ����
                await UserService.UpdateUser(user); // ����� ���������� ������������ � ��
            }

            await new MessageBox("����������� ������� ��������!", false).ShowDialog(contenContainer);
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

    //�������������� �������� ������ ������������
    private async void ApplyEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        //�������� �� ������������� ������������ �����
        if (!string.IsNullOrEmpty(UsernameTextBox.Text) && !string.IsNullOrEmpty(NicknameTextBox.Text))
        {
            if (CheckIfNicknameUnique(NicknameTextBox.Text))
            {
                User.Nickname = NicknameTextBox.Text;
                User.Biography = string.IsNullOrEmpty(BiographyTextBox.Text) ? "���������� �����������" : BiographyTextBox.Text;
                User.Username = UsernameTextBox.Text;

                UserService.UpdateUser(User);

                UsernameTextBox.Text = User.Username;
                BiographyTextBox.Text = User.Biography;
                NicknameTextBox.Text = User.Nickname;

                IsFieldsEnabled = !IsFieldsEnabled;
                OnPropertyChanged(nameof(IsFieldsEnabled));

                var contentContainer = this.GetVisualRoot() as ContentContainer;

                MessageBox messageBox = new MessageBox("������ ������� ���������!", false);
                await messageBox.ShowDialog(contentContainer);
            }
            else
            {
                ShowErrorMessage(message: "������������ � ����� Nickname ��� ����������", UserDataErrorMessage);
            }
        }
        else
        {
            ShowErrorMessage(message: "�� ��� ���� ���������", UserDataErrorMessage);
        }
    }

    private bool CheckIfNicknameUnique(string nickname) //��������� ���������� �� ��� ��������� ������� � ���� ������ 
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
                return; // �� ����� ������������ - �����

            using (var context = new PlanItContext())
            {
                var userSetting = context.Usersettings.Include(t => t.Colortheme)
                    .FirstOrDefault(u => u.Userid == AuthorizedUser.User.Id);

                var theme = context.Colorthemes.FirstOrDefault(t => t.Name == dbThemeName);
                if (theme == null)
                    return; // ���� �� ������� � ��

                userSetting.Colorthemeid = theme.Id;
                context.Usersettings.Update(userSetting);
                context.SaveChanges();

                // ��������� ��������� ����
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

                    MessageBox messageBox = new MessageBox("������ ������� ���������!", false);
                    await messageBox.ShowDialog(contentContainer);

                    PasswordTextBox.Clear();
                    RepeatPasswordTextBox.Clear();

                    PasswordErrorMessage.IsVisible = false;
                }
                else
                {
                    ShowErrorMessage(message: "������ ������ ��������� ����� �������� � ������� ���������, ����� � ����. ������", PasswordErrorMessage);
                }
            }
            else
            {
                ShowErrorMessage(message: "������ �� ���������", PasswordErrorMessage);
            }
        }
        else
        {
            ShowErrorMessage(message: "���� �� ���������", PasswordErrorMessage);
        }
    }

    //������� ������ � ��������������, ������ ������ � � ���� ����� ������� ������ �� ����
    private void RemoveChanges_ButtonClick(object sender, RoutedEventArgs e)
    {
        // �������� ������ ������ �� ����
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

public class ImageUploadResult //����-����� ��� ���������� ���������� ������ � ���� ����
{
    public Bitmap? Bitmap { get; set; }
    public string? FilePath { get; set; }
}
