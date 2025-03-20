using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab_03.WebProject.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0, 100000.000)]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; } // Đường dẫn đến hình ảnh đại diện
        public List<ProductImage>? ImageUrls { get; set; } // Danh sách các hình ảnh phụ
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
