using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using System.Collections.ObjectModel;

namespace Diploma_Ishchenko;

public partial class DetailNewsInfoUserControl : UserControl
{
    public News _selectedNews { get; set; }
    public ObservableCollection<Bitmap> NewsImagesCollection { get; } = new ObservableCollection<Bitmap>();
    public DetailNewsInfoUserControl()
    {
        InitializeComponent();
    }

    public DetailNewsInfoUserControl(News news)
    {
        InitializeComponent();
        _selectedNews = news;
        LoadImages();
        DataContext = this;
    }

    private void LoadImages()
    {
        NewsImagesCollection.Clear(); // Обязательно!

        var imagePaths = NewsImageService.GetImagesForNews(_selectedNews.Id);

        foreach (var path in imagePaths)
        {
            if (System.IO.File.Exists(path))
            {
                NewsImagesCollection.Add(new Bitmap(path));
            }
        }
    }


    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new NewsUserControl();
    }
}