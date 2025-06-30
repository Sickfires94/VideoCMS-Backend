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

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetByIdAsync(tagId);
        }

        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            return await _tagRepository.GetByNameAsync(tagName);
        }


        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAll();
        }


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

        public async Task<Tag> GetOrCreateTagByName(string tagName)
        {
            //Attempt to create tag
            Tag tag = await CreateTagAsync(new Tag { tagName = tagName });

            if(tag == null) await GetTagByNameAsync(tagName);

            return tag;
        }
    }
}
