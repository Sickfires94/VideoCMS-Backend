using Backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services.Interface
{
    /// <summary>
    /// Defines the contract for tag-related business operations.
    /// </summary>
    public interface ITagService
    {
        Task<Tag?> CreateTagAsync(Tag tag);

        Task<Tag?> GetTagByIdAsync(int tagId);

        Task<Tag?> GetTagByNameAsync(string tagName);

        Task<IEnumerable<Tag>> GetAllTagsAsync();

        Task<Tag?> UpdateTagAsync(Tag tag);

        Task<bool> DeleteTagAsync(int tagId);

        Task<bool> IsTagNameUniqueAsync(string tagName, int? excludeTagId = null);
        Task<Tag> GetOrCreateTagByName(string tagName);
    }
}
