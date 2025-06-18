using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Backend.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly VideoManagementApplicationContext _context;

        public CategoryRepository(VideoManagementApplicationContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _context.categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task DeleteAsync(int categoryId)
        {
            await _context.categories.Where(c => c.categoryId == categoryId).ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.categories.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllParentsHierarchyAsync(int categoryId)
        {
            var categories = new List<Category>();
            int? currentCategoryId = categoryId;

            while (currentCategoryId.HasValue)
            {
                var category = await _context.categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.categoryId == currentCategoryId.Value);
                if (category != null)
                {
                    categories.Add(category);
                    currentCategoryId = category.categoryParentId;
                }
                else
                {
                    currentCategoryId = null; // Exit loop if no parent found
                }
            }

            return categories;
        }


        public async Task<IEnumerable<Category>> GetAllImmediateChildrenAsync(int categoryId)
        {
            var children = await _context.categories // Assuming 'Categories' is the DbSet property in your DbContext for Category entities
                                         .AsNoTracking() // Recommended for read-only operations to improve performance
                                         .Where(c => c.categoryParentId == categoryId) // Filter by the parent's ID
                                         .ToListAsync(); // Execute the query and get the results as a list

            return children;
        }

        public async Task<IEnumerable<Category>> GetAllChildrenAsync(int categoryId)
        {
            var descendants = new List<Category>();
            var directChildren = await GetAllImmediateChildrenAsync(categoryId); // Get immediate children

            foreach (var child in directChildren)
            {
                descendants.Add(child);
                // Recursively add children of this child
                var nestedChildren = await GetAllChildrenAsync(child.categoryId);
                descendants.AddRange(nestedChildren);
            }
            return descendants;
        }


        public async Task<Category?> GetByIdAsync(int id) // Using Task<Category?> for nullable return
        {
            // FindAsync is efficient for primary key lookups.
            // It first checks the change tracker and then the database.
            return await _context.categories.FindAsync(id);
        }

        public async Task<Category?> GetByNameAsync(string name) // Using Task<Category?> for nullable return
        {
            return await _context.categories
                                 .AsNoTracking() // Recommended for read-only retrieval unless you plan to modify it immediately
                                 .FirstOrDefaultAsync(c => c.categoryName.ToLower() == name.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasChildrenAsync(int categoryId)
        {
            return await _context.categories.AnyAsync(c => c.categoryParentId == categoryId);
        }


    }
}

