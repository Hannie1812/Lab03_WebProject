using Lab_03.WebProject.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_03.WebProject.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category) // Bao gồm thông tin danh mục
                .Include(p => p.ImageUrls) // Bao gồm danh sách ảnh phụ
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category) // Lấy thông tin danh mục
                .Include(p => p.ImageUrls) // Lấy danh sách ảnh phụ
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        /*public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }*/
        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products
                .Include(p => p.ImageUrls) // Bao gồm danh sách ảnh phụ
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null) return;

            // Cập nhật thông tin sản phẩm
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            // Cập nhật ảnh chính (giữ ảnh cũ nếu không có ảnh mới)
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                existingProduct.ImageUrl = product.ImageUrl;
            }

            // Cập nhật danh sách ảnh phụ (nếu có ảnh mới)
            if (product.ImageUrls != null && product.ImageUrls.Any())
            {
                // Xóa ảnh cũ không còn trong danh sách mới
                var imagesToRemove = existingProduct.ImageUrls
                    .Where(img => !product.ImageUrls.Any(newImg => newImg.Url == img.Url))
                    .ToList();

                _context.ProductImages.RemoveRange(imagesToRemove);

                // Thêm ảnh phụ mới
                foreach (var newImage in product.ImageUrls)
                {
                    if (!existingProduct.ImageUrls.Any(img => img.Url == newImage.Url))
                    {
                        _context.ProductImages.Add(new ProductImage { ProductId = product.Id, Url = newImage.Url });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

    }
}
