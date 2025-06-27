using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете име.")]
        [StringLength(50, ErrorMessage = "Името трябва да бъде до 50 символа.")]
        [Column("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Моля, въведете фамилия.")]
        [StringLength(50, ErrorMessage = "Фамилията трябва да бъде до 50 символа.")]
        [Column("LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Моля, въведете имейл.")]
        [EmailAddress(ErrorMessage = "Моля, въведете валиден имейл.")]
        [Column("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете парола.")]
        [MinLength(6, ErrorMessage = "Паролата трябва да бъде поне 6 символа.")]
        [RegularExpression(@"^(?=.*[A-Z]).+$", ErrorMessage = "Паролата трябва да съдържа поне една главна буква.")]
        [Column("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Моля, изберете роля.")]
        [Column("Role")]
        public string Role { get; set; } = "User"; // Роля по подразбиране
    }
}