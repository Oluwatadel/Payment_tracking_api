using BC = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

namespace PaymentTracker.Services
{
    public interface IUserService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User?> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int userId);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public UserService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                return null;

            if (!BC.Verify(request.Password, user.PasswordHash))
                return null;

            var token = _jwtService.GenerateToken(user);
            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Token = token,
                Role = user.Role.ToString()
            };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            var hasUsername = await _userRepository.ExistsByUsernameAsync(request.Username);
            if (hasUsername)
                throw new InvalidOperationException("Username already exists");

            var hasPhone = await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber);
            if (hasPhone)
                throw new InvalidOperationException("Phone number already exists");

            var user = new User
            {
                Username = request.Username,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BC.HashPassword(request.Password),
                Role = UserRole.User
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<User?> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(request.Username))
            {
                if (await _userRepository.ExistsByUsernameAsync(request.Username, userId))
                    throw new InvalidOperationException("Username already exists");
                user.Username = request.Username;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, userId))
                    throw new InvalidOperationException("Phone number already exists");
                user.PhoneNumber = request.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BC.HashPassword(request.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null)
                return false;

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}
