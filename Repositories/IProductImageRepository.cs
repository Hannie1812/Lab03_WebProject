using Lab_03.WebProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab_03.WebProject.Repositories
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetAllAsync();
        Task<ProductImage> GetByIdAsync(int id);
        Task AddAsync(ProductImage productImage);
        Task UpdateAsync(ProductImage productImage);
        Task DeleteAsync(int id);
    }
}
