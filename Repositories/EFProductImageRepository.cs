using Lab_03.WebProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab_03.WebProject.Repositories
{
    public class EFProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductImage>> GetAllAsync()
        {
            return await _context.ProductImages.ToListAsync();
        }

        public async Task<ProductImage> GetByIdAsync(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public async Task AddAsync(ProductImage productImage)
        {
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductImage productImage)
        {
            _context.ProductImages.Update(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
        }
    }
}
