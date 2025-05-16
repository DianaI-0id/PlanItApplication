using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Diploma_Ishchenko;

public partial class CommunityRules : Window
{
    public CommunityRules()
    {
        InitializeComponent();
    }

    private void OK_ButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}