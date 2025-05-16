using bobscoffee_api.Models;
using bobscoffee_api.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace bobscoffee_api.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);
        Task<User?> RegisterAsync(User user, string plainPassword, string qrCodeDirectory);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly BobsCoffeeContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IQrCodeGenerator _qrCodeGenerator;

        public AuthService(
            BobsCoffeeContext context,
            IPasswordHasher passwordHasher,
            IQrCodeGenerator qrCodeGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _qrCodeGenerator = qrCodeGenerator;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            return _passwordHasher.Verify(password, user.PasswordHash)
                ? user
                : null;
        }


        public async Task<User?> RegisterAsync(User user, string plainPassword, string qrCodeDirectory)
        {
            if (await UserExistsAsync(user.Username))
                return null;

            // Hash password
            user.PasswordHash = _passwordHasher.Hash(plainPassword);

            // Generate QR code
            user.QrCodePath = _qrCodeGenerator.Generate(
        data: user.Username,
        directoryPath: qrCodeDirectory,
        fileName: $"user_{Guid.NewGuid()}"
    );

            // Set created date
            user.CoffeeCount = 0;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }
    }

    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public class BCryptPasswordHasher : IPasswordHasher
    {
        private readonly int _workFactor;

        public BCryptPasswordHasher(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(
                password,
                BCrypt.Net.BCrypt.GenerateSalt(_workFactor)
            );
        }

        public bool Verify(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}