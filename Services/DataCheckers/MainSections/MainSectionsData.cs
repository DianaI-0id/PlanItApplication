using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.DataCheckers.MainSections
{
    public class MainSectionsData
    {
        public static ObservableCollection<MainSection> Sections { get; } = new()
        {
            new MainSection
            {
                Title = "Сообщество",
                BackgroundBrush = Application.Current.FindResource("BorderBackground1") as Brush
            },
            new MainSection
            {
                Title = "Мои цели",
                BackgroundBrush = Application.Current.FindResource("BorderBackground4") as Brush
            },
             new MainSection
            {
                Title = "Мои задачи",
                BackgroundBrush = Application.Current.FindResource("BorderBackground3") as Brush
            },
              new MainSection
            {
                Title = "Совместные цели",
                BackgroundBrush = Application.Current.FindResource("BorderBackground1") as Brush
            }
            // Добавьте другие разделы
        };
    }
}
