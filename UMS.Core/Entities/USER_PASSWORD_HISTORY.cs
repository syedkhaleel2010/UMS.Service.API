using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Core.Entities
{
    [Table("USER_PASSWORD_HISTORY", Schema = "dbo")]
    public class USER_PASSWORD_HISTORY : EntityBase
    {
        public USER_PASSWORD_HISTORY()
        {
        }

        [Required]
        [MaxLength(36)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
