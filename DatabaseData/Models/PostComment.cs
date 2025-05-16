using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.DatabaseData.Models
{
    public class PostComment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public bool IsCurrentUserComment { get; set; }

        [NotMapped]
        public bool IsCurrentUserAdmin { get; set; }

        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}
