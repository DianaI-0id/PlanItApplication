using Diploma_Ishchenko.DatabaseData.Context;
using Diploma_Ishchenko.DatabaseData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.Services.PostgresServices
{
    public static class PostsService
    {
        public static List<Post> LoadPosts()
        {
            using (var context = new PlanItContext())
            {
                var posts = context.Posts
                    .Include(u => u.User)
                    .Include(p => p.PostImages)
                    .Include(p => p.PostComments)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                foreach (var post in posts)
                {
                    foreach (var comment in post.PostComments)
                    {
                         comment.IsCurrentUserComment = AuthorizedUser.User.Id == comment.UserId;
                    }
                }

                return posts.ToList();
            }
        }

        public static List<PostComment> LoadComments(int postId)
        {
            using (var context = new PlanItContext())
            {
                var posts = context.PostComments
                    .Include(c => c.User) 
                    .Where(c => c.PostId == postId)
                    .ToList();

                return posts.ToList();
            }
        }

        // Метод для удаления комментария по его Id
        public static async Task DeleteComment(int commentId)
        {
            using (var context = new PlanItContext())
            {
                var comment = await context.PostComments.FindAsync(commentId);
                if (comment != null)
                {
                    context.PostComments.Remove(comment);
                    await context.SaveChangesAsync();
                }
            }
        }
            // Добавляем новый метод для создания поста
        public static async Task CreatePost(Post post)
        {
            using var context = new PlanItContext(); 
            {
                post.Id = context.Posts.Any() ? context.Posts.Max(u => u.Id) + 1 : 1;

                context.Posts.Add(post);
                await context.SaveChangesAsync();
            };
        }

        public static async Task DeletePost(Post post)
        {
            using var context = new PlanItContext();
            {
                context.Posts.Remove(post);
                await context.SaveChangesAsync();
            };
        }
    }
}
