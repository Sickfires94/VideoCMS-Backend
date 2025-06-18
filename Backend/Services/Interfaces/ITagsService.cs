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
        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="tag">The Tag DTO to create.</param>
        /// <returns>The created Tag if successful, otherwise null (e.g., if the tag name is not unique).</returns>
        Task<Tag?> CreateTagAsync(Tag tag);

        /// <summary>
        /// Retrieves a tag by its unique ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag to retrieve.</param>
        /// <returns>The Tag DTO if found, otherwise null.</returns>
        Task<Tag?> GetTagByIdAsync(int tagId);

        /// <summary>
        /// Retrieves a tag by its name.
        /// </summary>
        /// <param name="tagName">The name of the tag to retrieve.</param>
        /// <returns>The Tag DTO if found, otherwise null.</returns>
        Task<Tag?> GetTagByNameAsync(string tagName);

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>An enumerable collection of all Tag DTOs.</returns>
        Task<IEnumerable<Tag>> GetAllTagsAsync();

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <param name="tag">The Tag DTO with updated values. The tagId must be valid.</param>
        /// <returns>The updated Tag DTO if successful, otherwise null (e.g., if not found, or updated name is not unique).</returns>
        Task<Tag?> UpdateTagAsync(Tag tag);

        /// <summary>
        /// Deletes a tag by its ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag to delete.</param>
        /// <returns>True if the tag was successfully deleted, false if not found.</returns>
        Task<bool> DeleteTagAsync(int tagId);

        /// <summary>
        /// Checks if a tag name is unique.
        /// </summary>
        /// <param name="tagName">The name to check for uniqueness.</param>
        /// <param name="excludeTagId">Optional. If provided, this tag ID will be excluded from the uniqueness check (useful for updates).</param>
        /// <returns>True if the name is unique, false otherwise.</returns>
        Task<bool> IsTagNameUniqueAsync(string tagName, int? excludeTagId = null);
    }
}
