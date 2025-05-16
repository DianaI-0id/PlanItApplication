using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Svg;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.Services.PostgresServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class ContentContainer : Window, INotifyPropertyChanged
{
    private Avalonia.Point _mousePosition;
    private bool _isDragging;
    public bool IsSideMenuVisible { get; set; } = false;
    public bool IsAdmin { get; set; } //�������� ��������� ������ � ����������� �� ������� ������������

    public ObservableCollection<Colortheme> ColorThemesCollection { get; set; } = new ObservableCollection<Colortheme>();

    public ContentContainer()
    {
        InitializeComponent();
        ContentControlContainer.Content = new CommunityUserControl();

        IsAdmin = AuthorizedUser.User.Roleid == 2 ? true : false;
        OnPropertyChanged(nameof(IsAdmin));

        CheckIfSubscriptionActive();
        LoadThemesInComboBox();

        DataContext = this;
    } 

    private void LoadThemesInComboBox()
    {
        using (var context = new PlanItContext())
        {
            var themes = context.Colorthemes.ToList();

            ColorThemesCollection.Clear();

            foreach (var item in themes)
            {
                ColorThemesCollection.Add(item);
            }
        }
    }

    //������� �������� �� �� ������� �� ��������. ���� ��, �� ����� �������� ������ ������������
    public async void CheckIfSubscriptionActive()
    {
        var user = UserService.GetUserByEmail(AuthorizedUser.User.Email);
        if (DateTime.Now > user.SubscriptionEndDate)
        {
            user.HasActiveSubscription = false;
            user.SubscriptionStartDate = null;
            user.SubscriptionEndDate = null;

            await UserService.UpdateUser(user);

            //� ������� ��������� � ��� ��� �������� �������
            var msBox =  new MessageBox(message: "\t��������! �������� ����� �������� �����������. \n\t����� ����� �������� ������ �� ���� �������� ����������, �������� �������� � ������ ��������. \n\t������ � �������� ����� ����� ����� �������� ����� ���������� ��������", false);
            await msBox.ShowDialog(this);

            AuthorizedUser.User.HasActiveSubscription = false; //�������� � ��� �������� ��������� ������ �������� ���� ��� �������
        }
    }

    private void ChangeSideMenuVisiblity_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        IsSideMenuVisible = !IsSideMenuVisible;
        if (IsSideMenuVisible == false)
        {
            SideMenuBorder.Width = 70;
        }
        else
        {
            SideMenuBorder.Width = 235;
        }
        OnPropertyChanged(nameof(IsSideMenuVisible));
    }

    private void OpenCommunity_ButtonClick(object sender, RoutedEventArgs e)
    {
        ContentControlContainer.Content = new CommunityUserControl();
    }

    private void OpenMyGoals_ButtonClick(object sender, RoutedEventArgs e)
    {
        if ((bool)!AuthorizedUser.User.HasActiveSubscription)
        {
            ShowErrorMessageBox(message: "������ ������ �������� ������ �� ��������");
            return;
        }
        ContentControlContainer.Content = new PersonalGoalsUserControl();
    }

    private void ShowBonusProgram_ButtonClick(object sender, RoutedEventArgs e)
    {
        //if ((bool)!AuthorizedUser.User.HasActiveSubscription)
        //{
        //    ShowErrorMessageBox(message: "������ ������ �������� ������ �� ��������");
        //    return;
        //}
        ContentControlContainer.Content = new BonusProgramUserControl();
    }

    private void OpenMyAnalytics_ButtonClick(object sender, RoutedEventArgs e)
    {
        ContentControlContainer.Content = new AnalyticsUserControl();
    }

    private void ShowMyBonuses_ButtonClick(object sender, RoutedEventArgs e)
    {
        if ((bool)!AuthorizedUser.User.HasActiveSubscription)
        {
            ShowErrorMessageBox(message: "������ ������ �������� ������ �� ��������");
            return;
        }
        ContentControlContainer.Content = new MyBonusesUserControl();
    }

    private void OpenMyTasks_ButtonClick(object sender, RoutedEventArgs e)
    {
        ContentControlContainer.Content = new PersonalTesksUserControl();
    }

    private void OpenMyProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        ContentControlContainer.Content = new UserProfileUserControl();
    }

    private void OpenMyPosts_ButtonClick(object sender, RoutedEventArgs e)
    {
        if ((bool)!AuthorizedUser.User.HasActiveSubscription)
        {
            ShowErrorMessageBox(message: "������ ������ �������� ������ �� ��������");
            return;
        }
        ContentControlContainer.Content = new MyPostsUserControl();
    }

    //��� ����� ������ � ������� ���������� � �������� ����
    private void ShowTelegramBotInfo_ButtonClick(object sender, RoutedEventArgs e)
    {
        if ((bool)!AuthorizedUser.User.HasActiveSubscription)
        {
            ShowErrorMessageBox(message: "������ ������ �������� ������ �� ��������");
            return;
        }
        //ContentControlContainer.Content = new MyPostsUserControl();
    }

    private void ShowNews_ButtonClick(object sender, RoutedEventArgs e)
    {
        ContentControlContainer.Content = new NewsUserControl();
    }

    private void ShowErrorMessageBox(string message)
    {
        var messageBox = new MessageBox(message, false);
        messageBox.ShowDialog(this);
    }

    private async void ShowSubscriptionInfo_ButtonClick(object sender, RoutedEventArgs e)
    {
        SubscriptionInfo subscription = new SubscriptionInfo();
        var result = await subscription.ShowDialog<bool>(this);

        if (result) //������� ���������. ���� true - ��������� � ���������� ��������
        {
            //���� � ������������ ��� ������� ��������, ��� ������ ��������� ����
            if ((bool)AuthorizedUser.User.HasActiveSubscription)
            {
                var messageBox = new MessageBox("� ��� ��� ���� ��������!", false);
                await messageBox.ShowDialog(this);
            }
            else
            {
                // ������� UserControl ������ ContentControlContainer
                var userControl = ContentControlContainer.GetVisualDescendants()
                    .OfType<UserControl>()
                    .FirstOrDefault();

                ContentControlContainer.Content = new SubscribeUserControl(userControl);
            }
        }
    }

    private void Logout_ButtonClick(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void ExitFromApp_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (this.VisualRoot is Window window)
        {
            window.Close(); // ��������� ������� ����
        }
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // ���������, �� ��������� �� ������� �� ComboBox ��� ��� �������� ���������
        if (e.Source is ComboBox || (e.Source as Control)?.FindAncestorOfType<ComboBox>() != null)
        {
            return;
        }

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
        // ����� ��������� ��� PointerReleased, ���� ��� �� �����������
        if (e.Source is ComboBox || (e.Source as Control)?.FindAncestorOfType<ComboBox>() != null)
        {
            return;
        }

        _isDragging = false;
    }

    //��� ������������ � �������� ���� ��� ���������
    private async void SideMenuBorder_PointerEntered(object sender, PointerEventArgs e)
    {
        // �� ����������, ���� ���� ��� �������
        if (IsSideMenuVisible) return;

        IsSideMenuVisible = true;
        SideMenuBorder.Width = 235;
        OnPropertyChanged(nameof(IsSideMenuVisible));
    }

    private async void SideMenuBorder_PointerExited(object sender, PointerEventArgs e)
    {
        // ���������, ��������� �� ������ ��� ��� ������ ����
        var position = e.GetPosition(SideMenuBorder);
        if (position.X >= 0 && position.X <= SideMenuBorder.Bounds.Width &&
            position.Y >= 0 && position.Y <= SideMenuBorder.Bounds.Height)
        {
            return; // ������ ��� ��� ������, �� �����������
        }

        // �� �����������, ���� ���� ��� �������
        if (!IsSideMenuVisible) return;

        IsSideMenuVisible = false;
        SideMenuBorder.Width = 70;
        OnPropertyChanged(nameof(IsSideMenuVisible));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}