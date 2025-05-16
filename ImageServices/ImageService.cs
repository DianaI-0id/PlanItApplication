using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.ImageServices
{
    public static class ImageService
    {
        static ImageService()
        {
            EnsureDefaultStructure();
        }

        private static void EnsureDefaultStructure()
        {
            string defaultDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "Avatars",
                "default");

            Directory.CreateDirectory(defaultDir);

            string defaultAvatar = Path.Combine(defaultDir, "no-profile-picture.png");
            if (!File.Exists(defaultAvatar))
            {
                using var bitmap = new RenderTargetBitmap(new PixelSize(150, 150));
                using (var ctx = bitmap.CreateDrawingContext())
                {
                    var brush = new SolidColorBrush(0xFFDDDDDD);
                    ctx.FillRectangle(brush, new Rect(0, 0, 150, 150));
                }
                bitmap.Save(defaultAvatar);
            }
        }

        public static async Task<ImageUploadResult?> ProcessImageUpload(
            int userId,
            Window parentWindow,
            string targetSubfolder = "Avatars",
            string filePrefix = "avatar")
        {
            try
            {
                var filePath = await GetImagePathFromDialog(parentWindow);
                if (filePath == null) return null;

                if (!IsValidImage(filePath))
                    throw new InvalidDataException("Неподдерживаемый формат изображения");

                var newPath = SaveImageForUser(filePath, userId, targetSubfolder, filePrefix);
                var bitmap = new Bitmap(newPath);

                return new ImageUploadResult { Bitmap = bitmap, FilePath = newPath };
            }
            catch (Exception ex)
            {
                await new MessageBox(ex.Message, false).ShowDialog(parentWindow);
                return null;
            }
        }


        //public static Bitmap? LoadUserImage(int userId, string targetSubfolder = "Avatars", string filePrefix = "avatar")
        //{
        //    try
        //    {
        //        var path = GetUserImagePath(userId, targetSubfolder, filePrefix);
        //        return path != null ? new Bitmap(path) : LoadDefaultImage();
        //    }
        //    catch
        //    {
        //        return LoadDefaultImage();
        //    }
        //}

        public static Bitmap? LoadUserImage(int userId, string? avatarPath = null, string targetSubfolder = "Avatars", string filePrefix = "avatar")
        {
            try
            {
                if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
                    return new Bitmap(avatarPath);

                var path = GetUserImagePath(userId, targetSubfolder, filePrefix);
                return path != null ? new Bitmap(path) : LoadDefaultImage();
            }
            catch
            {
                return LoadDefaultImage();
            }
        }

        private static Bitmap? LoadDefaultImage()
        {
            var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars", "default", "no-profile-picture.png");

            return File.Exists(defaultPath) ? new Bitmap(defaultPath) : LoadEmbeddedFallback();
        }

        private static Bitmap? LoadEmbeddedFallback()
        {
            try
            {
                var uri = new Uri("avares://Diploma_Ishchenko/Assets/placeholder.png");
                return new Bitmap(AssetLoader.Open(uri));
            }
            catch
            {
                // Fallback: создаем синий пиксель 1x1
                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                bw.Write(new byte[] { 0x42, 0x4D, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x00, 0x00, 0x00 });
                bw.Write(new byte[24]);
                bw.Write(new byte[] { 0x00, 0x00, 0xFF });
                ms.Position = 0;
                return new Bitmap(ms);
            }
        }

        private static async Task<string?> GetImagePathFromDialog(Window parent)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Изображения", Extensions = { "jpg", "png", "webp" } }
                }
            };

            var result = await dialog.ShowAsync(parent);
            return result?.FirstOrDefault();
        }

        private static string SaveImageForUser(string sourcePath, int userId, string subfolder, string filePrefix)
        {
            var targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", subfolder, $"User_{userId}");
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(targetDir, $"{filePrefix}.*"))
                File.Delete(file);

            var extension = Path.GetExtension(sourcePath).ToLower();
            var newPath = Path.Combine(targetDir, $"{filePrefix}{extension}");

            File.Copy(sourcePath, newPath, true);
            return newPath;
        }

        private static bool IsValidImage(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                byte[] header = new byte[4];
                stream.Read(header, 0, 4);

                return header switch
                {
                [0xFF, 0xD8, ..] => true, // JPEG
                [0x89, 0x50, 0x4E, 0x47] => true, // PNG
                [0x52, 0x49, 0x46, 0x46] => true, // WEBP
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private static string? GetUserImagePath(int userId, string subfolder, string filePrefix)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", subfolder, $"User_{userId}");

            // Проверяем существование директории
            if (!Directory.Exists(dir))
                return null;

            var files = Directory.GetFiles(dir, $"{filePrefix}.*");
            return files.FirstOrDefault();
        }
    }
}
