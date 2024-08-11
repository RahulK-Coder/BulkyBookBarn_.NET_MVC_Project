using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key] //we are describing Id as Primary key
        public int Id { get; set; }

        [Required] //it will have not null settings
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1,100,ErrorMessage = "Display Order must be between 1-100.")]
        public int DisplayOrder { get; set; }
    }
}
