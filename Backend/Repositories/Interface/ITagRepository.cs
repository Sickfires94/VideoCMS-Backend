// Backend.Repositories.Interface/ITagRepository.cs
using Backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITagRepository
{
    Task<Tag> CreateAsync(Tag tag);
    Task DeleteAsync(int tagId);
    Task<IEnumerable<Tag>> GetAll(); // Changed to GetAll to match your repo signature
    Task<Tag?> GetByIdAsync(int id); // Changed to nullable Task<Tag?>
    Task<Tag?> GetByNameAsync(string name); // Changed to nullable Task<Tag?>
    Task SaveChangesAsync();
}