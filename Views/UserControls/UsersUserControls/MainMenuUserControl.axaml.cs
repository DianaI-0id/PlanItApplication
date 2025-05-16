using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;

namespace Diploma_Ishchenko;

public partial class MainMenuUserControl : UserControl
{
    private ScrollViewer _scrollViewer;

    public MainMenuUserControl()
    {
        InitializeComponent();

        // Подписываемся на событие Loaded
        this.AttachedToVisualTree += OnAttachedToVisualTree;

        DataContext = this;
    }

    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        // Находим ScrollViewer после загрузки контрола
        _scrollViewer = this.GetVisualDescendants()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var scrollViewer = this.GetVisualDescendants()
                             .OfType<ScrollViewer>()
                             .FirstOrDefault(sv => sv.Name != "PART_ScrollViewer");

        if (scrollViewer != null)
        {
            scrollViewer.Offset += new Vector(-e.Delta.Y * 100, 0);
            e.Handled = true;
        }
    }

}