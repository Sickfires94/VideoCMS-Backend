using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly VideoManagementApplicationContext _context; // Replace YourDbContext with your actual DbContext name

        public UserRepository(VideoManagementApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user entity to add. Its CreatedDate will be set.</param>
        /// <returns>The created User entity (now tracked by the context).</returns>
        public async Task<User> CreateAsync(User user)
        {
            user.userCreatedDate = DateTime.UtcNow; // Set creation date
            user.userUpdatedDate = DateTime.UtcNow; // Set initial update date
            Debug.WriteLine("role before saving: " + user.role);
            _context.users.Add(user);
            await _context.SaveChangesAsync(); // Persist changes to the database
            Debug.WriteLine("role after saving: " + user.role);
            return user;
        }

        /// <summary>
        /// Deletes a user from the database by its entity.
        /// </summary>
        /// <param name="user">The user entity to delete.</param>
        public async Task DeleteAsync(User user)
        {
            // Attach the entity if it's not already tracked, then mark for deletion.
            // Or use ExecuteDeleteAsync for better performance if you only need the ID.
            var existingUser = await _context.users.FindAsync(user.userId);
            if (existingUser != null)
            {
                _context.users.Remove(existingUser);
                await _context.SaveChangesAsync(); // Persist changes
            }
            // If using ExecuteDeleteAsync (more efficient for direct delete by ID):
            // await _context.Users.Where(u => u.userId == user.userId).ExecuteDeleteAsync();
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>An enumerable collection of all User entities.</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.users.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The User entity if found; otherwise, null.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            // Consider case-insensitive comparison if your database doesn't enforce it
            return await _context.users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.userEmail == email);
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The User entity if found; otherwise, null.</returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.users.FindAsync(id); // FindAsync is efficient for primary key
        }

        /// <summary>
        /// Checks if an email address is unique (not already in use by another user).
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if the email is unique; otherwise, false.</returns>
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.users.AnyAsync(u => u.userEmail == email);
        }

        /// <summary>
        /// Checks if a username is unique (not already in use by another user).
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if the username is unique; otherwise, false.</returns>
        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _context.users.AnyAsync(u => u.userName == username);
        }

        /// <summary>
        /// Saves all pending changes in the DbContext to the database.
        /// This method is usually called by a service layer after a series of operations.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- UPDATE Method (Added as it's a common repository function, though not in your original list) ---
        /// <summary>
        /// Updates an existing User entity in the database.
        /// Assumes the user entity passed is the complete updated entity.
        /// </summary>
        /// <param name="user">The User entity with updated values.</param>
        /// <returns>The updated User entity (now tracked by the context).</returns>
       
    }
}