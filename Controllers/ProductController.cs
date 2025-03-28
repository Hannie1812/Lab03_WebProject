using Lab_03.WebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Lab_03.WebProject.Repositories;
namespace Lab_03.WebProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ProductController> _logger; //Ghi log thông tin, cảnh báo, hoặc lỗi.

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<ProductController> logger) // Inject ILogger
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        // Hiển thị form thêm sản phẩm mới
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        // Xử lý thêm sản phẩm mới
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl, List<IFormFile>? imageUrls)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                if (imageUrls != null)
                {
                    product.ImageUrls = new List<ProductImage>();
                    foreach (var file in imageUrls)
                    {
                        var imagePath = await SaveImage(file);
                        product.ImageUrls.Add(new ProductImage { Url = imagePath });
                    }
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
        // Viết thêm hàm SaveImage
        /*private async Task<string> SaveImage(IFormFile image)
        {
            //Thay đổi đường dẫn theo cấu hình của bạn
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }*/
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream); //ghi file bất đồng bộ
            }

            return "/images/" + image.FileName;
        }

        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name",product.CategoryId);
            return View(product);
        }
        // Xử lý cập nhật sản phẩm
        /*[HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); //Xóa kiểm tra ModelState cho ImageUrl (vì nó có thể null).
        if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingProduct = await
               _productRepository.GetByIdAsync(id); 
                                                    
            if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }*/
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl, List<IFormFile>? newImageUrls, List<string>? existingImageUrls, bool deleteMainImage)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin sản phẩm
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            // Xóa ảnh chính nếu chọn "Xóa ảnh chính"
            if (deleteMainImage && !string.IsNullOrEmpty(existingProduct.ImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                existingProduct.ImageUrl = null;
            }

            // Cập nhật ảnh chính nếu có ảnh mới
            if (imageUrl != null)
            {
                existingProduct.ImageUrl = await SaveImage(imageUrl);
            }

            // Xử lý ảnh phụ
            if (existingProduct.ImageUrls == null)
            {
                existingProduct.ImageUrls = new List<ProductImage>();
            }

            // Xóa ảnh phụ không được giữ lại
            if (existingImageUrls != null)
            {
                var imagesToDelete = existingProduct.ImageUrls
                    .Where(img => !existingImageUrls.Contains(img.Url))
                    .ToList();

                foreach (var img in imagesToDelete)
                {
                    var imagePath = Path.Combine("wwwroot", img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    existingProduct.ImageUrls.Remove(img);
                }
            }

            else
            {
                existingProduct.ImageUrls.Clear();
            }

            // Thêm ảnh phụ mới
            if (newImageUrls != null)
            {
                foreach (var file in newImageUrls)
                {
                    var imagePath = await SaveImage(file);
                    existingProduct.ImageUrls.Add(new ProductImage { Url = imagePath });
                }
            }

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }


        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm
        /*[HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }*/
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Xóa ảnh chính nếu có
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var mainImagePath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(mainImagePath))
                    {
                        System.IO.File.Delete(mainImagePath);
                    }
                }

                // Xóa ảnh phụ
                if (product.ImageUrls != null && product.ImageUrls.Any())
                {
                    foreach (var image in product.ImageUrls)
                    {
                        var imagePath = Path.Combine("wwwroot", image.Url.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                // Xóa sản phẩm
                await _productRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError($"Lỗi khi xóa sản phẩm {id}: {ex.Message}");
                return BadRequest("Có lỗi xảy ra khi xóa sản phẩm.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(List<int> selectedProducts)
        {
            if (selectedProducts != null && selectedProducts.Any())
            {
                foreach (var id in selectedProducts)
                {
                    await _productRepository.DeleteAsync(id);
                }
            }
            return RedirectToAction("Index");
        }
    }
}