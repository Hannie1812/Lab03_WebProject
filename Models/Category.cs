﻿using System.ComponentModel.DataAnnotations;

namespace Lab_03.WebProject.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
