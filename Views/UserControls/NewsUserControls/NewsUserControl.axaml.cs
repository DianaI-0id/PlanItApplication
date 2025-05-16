using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Diploma_Ishchenko.DatabaseData.Models;
using Diploma_Ishchenko.ImageServices;
using Diploma_Ishchenko.Services.PostgresServices;
using System.Collections.ObjectModel;

namespace Diploma_Ishchenko;

public partial class NewsUserControl : UserControl
{
    public ObservableCollection<News> NewsCollection { get; set; } = new ObservableCollection<News>();

    public bool IsAdmin => AuthorizedUser.User.Roleid == 2;
    public NewsUserControl()
    {
        InitializeComponent();
        LoadNews();
        DataContext = this;
    }

    private void LoadNews()
    {
        NewsCollection.Clear();
        var newsList = NewsService.LoadNews();
        NewsCollection = new ObservableCollection<News>(newsList);
    }

    private void ShowDetailNews_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is News news)
        {
            var contentContainer = this.GetVisualRoot() as ContentContainer;
            contentContainer.ContentControlContainer.Content = new DetailNewsInfoUserControl(news);
        }  
    }

    private async void DeleteNews_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is News news)
        {
            NewsCollection.Remove(news);
            await NewsImageService.DeleteNews(news);
        }
    }

    private void AddNews_ButtonClick(object sender, RoutedEventArgs e)
    {
        var contentContainer = this.GetVisualRoot() as ContentContainer;
        contentContainer.ContentControlContainer.Content = new AddNewsUserControl();
    }

    private void ShowEditNews_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is News news)
        {
            var contentContainer = this.GetVisualRoot() as ContentContainer;
            contentContainer.ContentControlContainer.Content = new EditSelectedNewsUserControl(news);
        }
    }
}