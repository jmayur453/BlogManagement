using BlogManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogManagement.Helpers
{
    public class UserHelper
    {
        private readonly PasswordHasher<UserDetail> _passwordHasher = new();
        public string EncryptPassword(UserDetail user)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            return user.Password;
        }
        public bool VerifyPassword(UserDetail user, string password)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Success;
        }
    }
}
