using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class AddNewsUserControl : UserControl
{
    //������� ��� ����� ��� �������� ����������� �����������
    public class ImageInfo
    {
        public Bitmap Bitmap { get; set; }
        public string OriginalPath { get; set; } // ������ ���� � �����
        public string RelativePath { get; set; } // ���� � ���� � �� �����
    }

    public ObservableCollection<ImageInfo> Images { get; } = new ObservableCollection<ImageInfo>();

    public AddNewsUserControl()
    {
        InitializeComponent();
        ImagesListBox.ItemsSource = Images; // ������������� ��������
        DataContext = this;
    }

    private async void OnAddImageClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "�������� �����������",
                AllowMultiple = true,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "�����������", Extensions = { "jpg", "jpeg", "png", "webp" } }
                }
            };

            var result = await dialog.ShowAsync(contentContainer);

            if (result != null && result.Any())
            {
                foreach (var filePath in result)
                {
                    if (File.Exists(filePath))
                    {
                        Images.Add(new ImageInfo
                        {
                            Bitmap = new Bitmap(filePath),
                            OriginalPath = filePath
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await new MessageBox(ex.Message, false).ShowDialog(contentContainer);
        }
    }

    private void OnRemoveImageClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ImageInfo image)
        {
            Images.Remove(image);
        }
    }

    private async void OnAddNewsClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            await new MessageBox("������� ��������� �������", false).ShowDialog(contentContainer);
            return;
        }

        try
        {
            var imagePaths = Images.Select(i => i.OriginalPath).ToList();

            await NewsService.AddNewsWithImages(TitleTextBox.Text, ContentTextBox.Text, AuthorizedUser.User.Id, imagePaths);

            await new MessageBox("������� ������� ���������", false).ShowDialog(contentContainer);

            // ������� �����
            TitleTextBox.Text = string.Empty;
            ContentTextBox.Text = string.Empty;
            Images.Clear();

            //������������ � ������ ��������
            contentContainer.ContentControlContainer.Content = new NewsUserControl();
        }
        catch (Exception ex)
        {
            await new MessageBox($"������ ��� ���������� �������: {ex.Message}", false).ShowDialog(contentContainer);
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new NewsUserControl();
    }
}