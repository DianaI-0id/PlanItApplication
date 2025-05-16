using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Diploma_Ishchenko;

public partial class SubscriptionInfo : Window
{
    public SubscriptionInfo()
    {
        InitializeComponent();
    }

    private void Subscribe_ButtonClick(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void Back_ButtonClick(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}