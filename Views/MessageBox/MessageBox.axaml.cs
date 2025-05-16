using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace Diploma_Ishchenko;

public partial class MessageBox : Window, INotifyPropertyChanged
{
    public string BoxMessage { get; set; }
    public MessageBox()
    {
        InitializeComponent();
    }

    public MessageBox(string message, bool IsCancelVisible)
    {
        InitializeComponent();
        BoxMessage = message;
        CancelButton.IsVisible = IsCancelVisible;
        DataContext = this;
    }

    private void OK_ButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close(true);
    }

    private void Cancel_ButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close(false);
    }
}