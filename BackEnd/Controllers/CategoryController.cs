using AutoMapper;
using BackEnd.Data;
using BackEnd.DTO.Category;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        // Dependency injection of the database context
        public CategoryController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        // Get all categories from the database and return them as a response
        [Authorize(Roles = "Admin,User,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryDTOs = _mapper.Map<List<GetCategory>>(categories);
            return Ok(categoryDTOs);
        }
        // post method to add a new category to the database using the AddCategory DTO
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromForm] AddCategory category)
        {
            // Manually mapping the AddCategory DTO to the Category model
            //var newCategory = new Category();
            //newCategory.Name = category.Name;
            //newCategory.Description = category.Description;
            //newCategory.ImageUrl = category.ImageUrl;

            // Using AutoMapper to map the AddCategory DTO to the Category model
            var newCategory = _mapper.Map<Category>(category);
            await _context.Categories.AddAsync(newCategory);
            await _context.SaveChangesAsync();
            return Ok(category);
        }
        //Get a category by its ID from the database and return it as a response
        [Authorize(Roles = "Admin,User,Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetId(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }
        // Soft delete a category by setting its IsDeleted property to true and updating it in the database
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.IsDeleted = true;
            _context.Categories.Update(category); // No Await in update
            await _context.SaveChangesAsync();
            return Ok(category);
        }
        // Hard delete a category by removing it from the database
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("HardDelete/{id}")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }
        // Update a category by its ID using the EditCategory DTO and saving the changes to the database
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCatgory(Guid id,[FromForm] EditCategory UpdateCategoty)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = UpdateCategoty.Name;
            category.Description = UpdateCategoty.Description;
            category.ImageUrl = UpdateCategoty.ImageUrl;
            category.UpdatedAt = DateTime.Now;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return Ok(UpdateCategoty);
        }
    }
}
