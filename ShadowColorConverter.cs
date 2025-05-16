using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko
{
    public class ShadowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Получаем цвет через DynamicResourceExtension
            if (value is DynamicResourceExtension resource)
            {
                var color = Application.Current!.FindResource(resource.ResourceKey) as Color?;
                return new BoxShadows(
                    new BoxShadow { OffsetX = 5, OffsetY = 5, Blur = 10, Color = color ?? Colors.Black }
                );
            }
            return BoxShadows.Parse("5 5 10 0 #000000");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException(); // Обратная конвертация не нужна
    }
}
