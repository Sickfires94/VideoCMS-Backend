using Backend.DTOs;
using Backend.Repositories.Interface; // Assuming ITagRepository is in this namespace
using Backend.Services.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        /// <summary>
        /// Creates a new tag, ensuring its name is unique.
        /// </summary>
        /// <param name="tag">The Tag DTO to create.</param>
        /// <returns>The created Tag if successful, otherwise null.</returns>
        public async Task<Tag?> CreateTagAsync(Tag tag)
        {
            // Check for tag name uniqueness
            bool isUnique = await IsTagNameUniqueAsync(tag.tagName);
            if (!isUnique)
            {
                // A tag with the same name already exists
                return null;
            }

            var createdTag = await _tagRepository.CreateAsync(tag);
            return createdTag;
        }

        /// <summary>
        /// Retrieves a tag by its ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>The Tag DTO or null if not found.</returns>
        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetByIdAsync(tagId);
        }

        /// <summary>
        /// Retrieves a tag by its name.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The Tag DTO or null if not found.</returns>
        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            return await _tagRepository.GetByNameAsync(tagName);
        }

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>An enumerable collection of all tags.</returns>
        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAll();
        }

        /// <summary>
        /// Updates an existing tag, ensuring the updated name is unique.
        /// </summary>
        /// <param name="tag">The Tag DTO with updated values.</param>
        /// <returns>The updated Tag if successful, otherwise null.</returns>
        public async Task<Tag?> UpdateTagAsync(Tag tag)
        {
            var existingTag = await _tagRepository.GetByIdAsync(tag.tagId);
            if (existingTag == null)
            {
                // Tag not found
                return null;
            }

            // Check for tag name uniqueness if the name is being changed
            if (existingTag.tagName != tag.tagName)
            {
                bool isUnique = await IsTagNameUniqueAsync(tag.tagName, tag.tagId);
                if (!isUnique)
                {
                    // New tag name is not unique
                    return null;
                }
            }

            // Update the existing entity properties
            existingTag.tagName = tag.tagName;

            // Save changes to the database
            await _tagRepository.SaveChangesAsync();
            return existingTag;
        }

        /// <summary>
        /// Deletes a tag by its ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag to delete.</param>
        /// <returns>True if deleted, false if not found.</returns>
        public async Task<bool> DeleteTagAsync(int tagId)
        {
            var tagToDelete = await _tagRepository.GetByIdAsync(tagId);
            if (tagToDelete == null)
            {
                // Tag not found
                return false;
            }

            // In a real application, you might want to check for associated videos/content
            // before allowing deletion, or cascade delete relationships.
            // For now, it directly calls the repository delete.

            await _tagRepository.DeleteAsync(tagId);
            return true;
        }

        /// <summary>
        /// Checks if a tag name is unique (case-insensitive).
        /// If excludeTagId is provided, it's used to exclude the current tag during update checks.
        /// </summary>
        /// <param name="tagName">The name to check.</param>
        /// <param name="excludeTagId">ID of tag to exclude from check.</param>
        /// <returns>True if unique, false otherwise.</returns>
        public async Task<bool> IsTagNameUniqueAsync(string tagName, int? excludeTagId = null)
        {
            var existingTagsWithSameName = (await _tagRepository.GetAll())
                                            .Where(t => t.tagName.Equals(tagName, System.StringComparison.OrdinalIgnoreCase));

            // Exclude the tag being updated if excludeTagId is provided
            if (excludeTagId.HasValue)
            {
                existingTagsWithSameName = existingTagsWithSameName.Where(t => t.tagId != excludeTagId.Value);
            }

            // If no tags match after filters, the name is unique
            return !existingTagsWithSameName.Any();
        }
    }
}
