using Backend.DTOs;

namespace Backend.Services
{
    public class UserService
    {
        private readonly VideoManagementApplicationContext _context;

        public UserService(VideoManagementApplicationContext context)
        {
            _context = context;
        }


        public List<User> GetAllUsers()
        {

            System.Diagnostics.Debug.WriteLine("****************************************\n");
            return _context.users.ToList();
        }

        public void AddUser(User user)
        {
            _context.users.Add(user);
            _context.SaveChanges();
        }

    }
}
