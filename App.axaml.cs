using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Diploma_Ishchenko.DatabaseData.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Diploma_Ishchenko
{
    public partial class App : Application
    {
        private string _currentTheme = "DefaultRedTheme"; // ���� �� ���������
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 1. ��������� ��������� ���� �� ������
            var settings = LoadLocalSettings();
            SetTheme(settings.Theme);

            // 2. ������ - ����������� �������������
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //SetTestUser();
                desktop.MainWindow = new LoginWindow();
                //desktop.MainWindow = new SubscriptionInfo();
            }

            base.OnFrameworkInitializationCompleted();
        } 

        private void SetTestUser()
        {
            using (var context = new PlanItContext())
            {
                AuthorizedUser.User = context.Users.FirstOrDefault(u => u.Id == 1);
            }
        }

        public void SetTheme(string themeName)
        {
            try
            {
                // ���������� ����
                var themeUri = new Uri($"avares://Diploma_Ishchenko/AppColorThemes/{themeName}.axaml");
                var resourceInclude = new ResourceInclude(themeUri)
                {
                    Source = themeUri
                };

                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(resourceInclude);

                // ���������� � ��������� ����
                var settings = new LocalSettings { Theme = themeName };
                File.WriteAllText(GetSettingsPath(), JsonSerializer.Serialize(settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ ����: {ex.Message}");
            }
        }


        private string GetSettingsPath()
        {
            // �������� ���� � ����� AppData ������������
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Windows � �����������������[1][5][6]
            var dir = Path.Combine(appData, "Diploma_Ishchenko"); // �������� �� ��� ������ ����������

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, "settings.json");
        }

        public LocalSettings LoadLocalSettings()
        {
            var path = GetSettingsPath();
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<LocalSettings>(json) ?? new LocalSettings();
                }
                catch
                {
                    // ���� ���� ��������� - ���������� ��������� �� ���������
                    return new LocalSettings();
                }
            }
            return new LocalSettings();
        }

        private void SaveLocalSettings(LocalSettings settings)
        {
            var path = GetSettingsPath();
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}