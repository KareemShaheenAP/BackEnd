using AutoMapper;
using BackEnd.Data;
using BackEnd.DTO.Product;
using BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        public ProductController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        // get all products from the database and return them as a response
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();

            var productDTOs = _mapper.Map<List<GetProduct>>(products);
            return Ok(productDTOs);
        }
        // Get a product by its ID from the database and return it as a response
        [HttpGet("{id}")]
        public async Task<IActionResult> Getid(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        // soft delete a product by its ID from the database and return a response
        [HttpDelete("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.IsDeleted = true;
            _context.Products.Update(product); // No Await in update
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        // Hard delete a product by its ID from the database and return a response
        [HttpDelete("HardDelete/{id}")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        // Add a new product to the database using the AddProduct DTO and return a response
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] AddProduct product)
        {
            // Manually map the AddProduct DTO to the Product entity
            //var newproduct = new Product
            //{
            //    Name = product.Name,
            //    Description = product.Description,
            //    Price = product.Price,
            //    CategoryId = product.CategoryId
            //};
            // Automatically map the AddProduct DTO to the Product entity using AutoMapper
            var newproduct = _mapper.Map<Product>(product);
            await _context.Products.AddAsync(newproduct);
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        // Update a product by its ID from the database using the AddProduct DTO and return a response
        [HttpPut]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] EditProduct product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            _context.Products.Update(existingProduct); // No Await in update
            await _context.SaveChangesAsync();
            return Ok(product);
        }
    }
}
