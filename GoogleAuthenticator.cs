using Diploma_Ishchenko.DatabaseData.Models;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class GoogleAuthenticator
{
    private string ApplicationName = "PlanItApplication";

    string[] scopes =
    {
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/userinfo.profile"
    };

    public async Task<Userinfo> AuthenticateAsync()
    {
        UserCredential credential;

        // Путь к client_secret.json
        var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "client-secret.json");

        // Убедимся, что файл существует
        if (!File.Exists(credPath))
        {
            throw new FileNotFoundException("client-secret.json не найден по пути: " + credPath);
        }

        // Директория для хранения токенов
        var dataStorePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "GoogleAuth");

        // Очищаем старые данные перед новой авторизацией
        if (Directory.Exists(dataStorePath))
        {
            Directory.Delete(dataStorePath, true);  // Удаляем старые токены
        }

        // Загружаем файл client_secret.json
        using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
        {
            // Авторизация с параметром "prompt=select_account"
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(dataStorePath, true));  // Сохраняем токены в отдельной папке
        }

        // Создаем OAuth2 сервис
        var oauth2Service = new Oauth2Service(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Получаем информацию о пользователе
        var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

        return userInfo;  // Возвращаем данные о пользователе
    }
}
