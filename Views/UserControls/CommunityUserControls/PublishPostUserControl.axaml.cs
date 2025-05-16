using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;
using Avalonia.VisualTree;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class PublishPostUserControl : UserControl
{
    public class PostImageItem
    {
        public Bitmap Bitmap { get; set; }
        public string OriginalPath { get; set; }
    }

    public ObservableCollection<PostImageItem> PostImagesCollection { get; } = new();
    public PersonalTask _selectedTask { get; set; }

    public PublishPostUserControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    private async void Publish_Click(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        if (string.IsNullOrEmpty(PostTitleTextBox.Text))
        {
            await new MessageBox("Введите заголовок", false).ShowDialog(contentContainer);
            return;
        }

        var title = PostTitleTextBox.Text;
        var description = PostDescriptionTextBox.Text;
        try
        {
            // Сначала создаем и сохраняем пост
            var newPost = new Post
            {
                UserId = AuthorizedUser.User.Id,
                Title = title,
                Content = description,
                CreatedAt = DateTime.Now
            };

            // Сохраняем пост в БД и получаем реальный ID
            await PostsService.CreatePost(newPost); // ДОБАВИТЬ ЭТО ПЕРВЫМ!

            // Теперь newPost.Id содержит корректное значение
            foreach (var image in PostImagesCollection)
            {
                var targetPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    "Posts",
                    newPost.Id.ToString(),
                    Path.GetFileName(image.OriginalPath)
                );

                var directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(image.OriginalPath))
                {
                    File.Copy(image.OriginalPath, targetPath, overwrite: true);
                }
                else
                {
                    throw new FileNotFoundException($"Файл не найден: {image.OriginalPath}");
                }
                await PostImageService.AddImageToPostAsync(newPost.Id, targetPath); // ПЕРЕДАЕМ ТОЛЬКО ID
            }

            contentContainer.ContentControlContainer.Content = new CommunityUserControl();
        }
        catch (Exception ex)
        {
            await new MessageBox($"Ошибка: {ex.Message}", false).ShowDialog(contentContainer);
        }
    }


    private async void AddImages_Click(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        try
        {
            var dialog = new OpenFileDialog
            {
                AllowMultiple = true,
                Filters = new List<FileDialogFilter>
            {
                new() { Name = "Images", Extensions = { "jpg", "png", "webp" } }
            }
            };

            var files = await dialog.ShowAsync(contentContainer);

            if (files?.Any() == true)
            {
                foreach (var file in files)
                {
                    PostImagesCollection.Add(new PostImageItem
                    {
                        Bitmap = new Bitmap(file),
                        OriginalPath = file
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await new MessageBox($"Ошибка: {ex.Message}", false).ShowDialog(contentContainer);
        }
    }

    private void RemoveImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PostImageItem item)
        {
            PostImagesCollection.Remove(item);
        }
    }


    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        // Просто закрываем окно без сохранения
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new CommunityUserControl();
    }
}