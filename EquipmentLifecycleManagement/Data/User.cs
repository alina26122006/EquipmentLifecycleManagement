using System.ComponentModel.DataAnnotations;

namespace EquipmentLifecycleManager.Data
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } // В реальном приложении хранить хэш!

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } // "Admin", "Engineer", "Technician", "Manager"

        [MaxLength(200)]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}