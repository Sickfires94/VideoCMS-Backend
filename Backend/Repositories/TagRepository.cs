using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class TagRepository : ITagRepository
    {

        private readonly VideoManagementApplicationContext _context;

        public TagRepository(VideoManagementApplicationContext context)
        {
            _context = context;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            await _context.tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        public async Task DeleteAsync(int tagId)
        {
            await _context.tags
                .Where(t => t.tagId == tagId)
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<Tag>> GetAll()
        {
            return await _context.tags.ToListAsync();
        }

        public async Task<Tag> GetByIdAsync(int id)
        {
            return await _context.tags.FindAsync(id);
        }

        public Task<Tag> GetByNameAsync(string name)
        {
           return _context.tags
                .AsNoTracking() // Use AsNoTracking for read-only operations to improve performance
                .FirstOrDefaultAsync(t => t.tagName == name); // Assuming tagName is the property for the tag's name
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
    }
}
}
