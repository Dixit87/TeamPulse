using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Users")]
    public class TblUser
    {
        [Key]
        public int UserID { get; set; }

        public string FullName { get; set; }

        [Required]
        public string Email { get; set; } // Login ID

        [Required]
        public string Password { get; set; }

        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public virtual TblRole Role { get; set; } // Role ka naam laane ke liye

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}