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
    public static class NewsImageService
    {
        private static string GetFullImagePath(string relativePath) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", relativePath);

        public static async Task AddImageToNewsAsync(int newsId, string imagePath)
        {
            string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "News", newsId.ToString());
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);

            string ext = Path.GetExtension(imagePath);
            string newFileName = $"image_{Guid.NewGuid()}{ext}";
            string destPath = Path.Combine(imagesDir, newFileName);

            File.Copy(imagePath, destPath, overwrite: true);

            // Относительный путь теперь с двумя уровнями: News/{newsId}/filename
            string relativePath = Path.Combine("News", newsId.ToString(), newFileName);

            using (var context = new PlanItContext())
            {
                context.NewsImages.Add(new NewsImage
                {
                    NewsId = newsId,
                    ImagePath = relativePath
                });
                await context.SaveChangesAsync();
            }
        }

        private static string GetRelativeImagePath(string fullPath) =>
            Path.GetRelativePath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"),
                fullPath);

        public static List<string> GetImagesForNews(int newsId)
        {
            using (var context = new PlanItContext())
            {
                return context.NewsImages
                  .Where(x => x.NewsId == newsId)
                  .Select(x => GetFullImagePath(x.ImagePath))
                  .ToList();
            }
        }

        public static async Task DeleteImageAsync(int imageId)
        {
            using (var context = new PlanItContext())
            {
                var image = await context.NewsImages.FindAsync(imageId);

                if (image != null)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);

                    context.NewsImages.Remove(image);
                    await context.SaveChangesAsync();
                }
            } 
        }

        public static async Task UpdateNewsImagesAsync(int newsId, List<string> newImagePaths)
        {
            using (var context = new PlanItContext())
            {
                var existingImages = context.NewsImages.Where(x => x.NewsId == newsId).ToList();

                foreach (var image in existingImages)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                context.NewsImages.RemoveRange(existingImages);
                await context.SaveChangesAsync();

                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "News", newsId.ToString());
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                foreach (var imagePath in newImagePaths)
                {
                    string ext = Path.GetExtension(imagePath);
                    string newFileName = $"image_{Guid.NewGuid()}{ext}";
                    string destPath = Path.Combine(imagesDir, newFileName);

                    File.Copy(imagePath, destPath, overwrite: true);

                    string relativePath = Path.Combine("News", newsId.ToString(), newFileName);

                    context.NewsImages.Add(new NewsImage
                    {
                        NewsId = newsId,
                        ImagePath = relativePath
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        public static async Task DeleteImageByPathAsync(string relativePath)
        {
            using (var context = new PlanItContext())
            {
                var image = context.NewsImages.FirstOrDefault(i => i.ImagePath == relativePath);
                if (image != null)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);

                    context.NewsImages.Remove(image);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task DeleteNews(News news)
        {
            using (var context = new PlanItContext())
            {
                context.News.Remove(news);
                await context.SaveChangesAsync();
            }
        }

        public static async Task DeleteAllNewsImagesAsync(int newsId)
        {
            using (var context = new PlanItContext())
            {
                var images = context.NewsImages
                    .Where(x => x.NewsId == newsId)
                    .ToList();

                foreach (var image in images)
                {
                    var fullPath = GetFullImagePath(image.ImagePath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                context.NewsImages.RemoveRange(images);
                await context.SaveChangesAsync();
            }      
        }
    }
}
