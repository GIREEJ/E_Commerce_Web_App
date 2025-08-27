using ECommerceWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ECommerceWebApp.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();
        private readonly object _dummyUser = new(); // Required

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(_dummyUser, password);
        }

        public bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(_dummyUser, hashedPassword, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
