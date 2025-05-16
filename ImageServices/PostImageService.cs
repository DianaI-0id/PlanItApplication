using Avalonia.Media.Imaging;
using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.ImageServices
{
    public static class PostImageService
    {
        private static string GetFullImagePath(string relativePath) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", relativePath);

        public static Bitmap? LoadBitmapFromImagePath(string imagePath)
        {
            try
            {
                var fullPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    imagePath);

                Console.WriteLine($"Полный путь к изображению: {fullPath}"); // Для отладки

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine("Файл не существует!");
                    return null;
                }

                return new Bitmap(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return null;
            }
        }


        // PostImageService.cs
        public static async Task AddImageToPostAsync(int postId, string imagePath)
        {
            var relativePath = GetRelativeImagePath(imagePath);

            using var context = new PlanItContext();
            context.PostImages.Add(new PostImage
            {
                PostId = postId, // ИСПОЛЬЗУЕМ ПЕРЕДАННЫЙ ID
                ImagePath = relativePath
            });

            await context.SaveChangesAsync();
        }


        public static List<string> GetImagesForPost(int postId)
        {
            using (var context = new PlanItContext())
            {
                return context.PostImages
                    .Where(x => x.PostId == postId)
                    .Select(x => GetFullImagePath(x.ImagePath))
                    .ToList();
            }  
        }

        public static async Task DeleteImageAsync(int imageId)
        {
            using (var context = new PlanItContext())
            {
                var image = await context.PostImages.FindAsync(imageId);

                if (image != null)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);

                    context.PostImages.Remove(image);
                    await context.SaveChangesAsync();
                }
            }   
        }

        public static async Task DeleteAllPostImagesAsync(int postId)
        {
            using (var context = new PlanItContext())
            {
                var images = context.PostImages
                   .Where(x => x.PostId == postId)
                   .ToList();

                foreach (var image in images)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                context.PostImages.RemoveRange(images);
                await context.SaveChangesAsync();
            }
        }

        private static string GetRelativeImagePath(string fullPath) =>
            Path.GetRelativePath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"),
                fullPath);
    }
}
