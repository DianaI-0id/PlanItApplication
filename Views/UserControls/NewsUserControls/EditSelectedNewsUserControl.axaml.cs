using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using static Diploma_Ishchenko.AddNewsUserControl;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Diploma_Ishchenko;

public partial class EditSelectedNewsUserControl : UserControl
{
    private News _news;
    public ObservableCollection<ImageInfo> Images { get; } = new();

    private List<string> _originalImagePaths = new List<string>(); // Храним пути изначальных изображений

    public EditSelectedNewsUserControl()
    {
        InitializeComponent();
    }

    public EditSelectedNewsUserControl(News news)
    {
        InitializeComponent();
        _news = news;
        LoadExistingData();
        DataContext = _news;
        ImagesListBox.ItemsSource = Images;
    }

    private void LoadExistingData()
    {
        TitleTextBox.Text = _news.Title;
        ContentTextBox.Text = _news.Content;

        Images.Clear();

        var existingImages = NewsImageService.GetImagesForNews(_news.Id);

        foreach (var path in existingImages)
        {
            if (File.Exists(path))
            {
                Images.Add(new ImageInfo
                {
                    Bitmap = new Bitmap(path),
                    OriginalPath = path,
                    RelativePath = GetRelativePath(path)
                });
            }
        }

        // Обновляем список оригинальных путей после загрузки
        _originalImagePaths = Images.Select(i => i.OriginalPath).ToList();
    }


    private string GetRelativePath(string fullPath)
    {
        var imagesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        return Path.GetRelativePath(imagesRoot, fullPath);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Обновляем основную информацию
            _news.Title = TitleTextBox.Text;
            _news.Content = ContentTextBox.Text;
            await NewsService.UpdateNews(_news);

            var currentPaths = Images.Select(i => i.OriginalPath).ToList();

            // Определяем удалённые изображения
            var removedPaths = _originalImagePaths.Except(currentPaths).ToList();

            // Удаляем из базы и файловой системы
            foreach (var removedPath in removedPaths)
            {
                await NewsImageService.DeleteImageByPathAsync(removedPath);
            }

            // Добавляем новые изображения
            var newImages = Images.Where(i => !_originalImagePaths.Contains(i.OriginalPath)).ToList();
            foreach (var newImage in newImages)
            {
                await NewsImageService.AddImageToNewsAsync(_news.Id, newImage.OriginalPath);
            }

            // Обновляем список оригинальных путей
            _originalImagePaths = currentPaths;

            var contentContainer = this.GetVisualRoot() as ContentContainer;
            contentContainer.ContentControlContainer.Content = new NewsUserControl();
        }
        catch (Exception ex)
        {
            await new MessageBox($"Ошибка: {ex.Message}", false).ShowDialog(this.GetVisualRoot() as Window);
        }
    }

    private async void OnRemoveImageClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ImageInfo image)
        {
            // Удаляем из коллекции UI
            Images.Remove(image);

            // Удаляем из базы и файловой системы
            // Предполагаем, что ImageInfo содержит относительный путь
            if (!string.IsNullOrEmpty(image.RelativePath))
            {
                await NewsImageService.DeleteImageByPathAsync(image.RelativePath);
            }
        }
    }

    private async void OnAddImageClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;

        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображения",
                AllowMultiple = true,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Изображения", Extensions = { "jpg", "jpeg", "png", "webp" } }
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

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new NewsUserControl();
    }
}