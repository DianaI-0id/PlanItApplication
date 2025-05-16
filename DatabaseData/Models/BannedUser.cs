using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_Ishchenko.DatabaseData.Models
{
    public class BannedUser
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime? BanDate { get; set; }

        public DateTime? BanEnddate { get; set; }

        public string Reason { get; set; }

        public virtual User? User { get; set; }
    }
}
