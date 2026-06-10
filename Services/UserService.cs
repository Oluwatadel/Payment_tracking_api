using BC = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;
using PaymentTracker.Exceptions;

namespace PaymentTracker.Services
{
    public interface IUserService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid userId);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IAccountRepository accountRepository, IJwtService jwtService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation($"============================{DateTime.Now:dd-MM-yyyy, HH:mm:ss}=================================");
            _logger.LogInformation($"Attempting login for username: {request.Username}");
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("User not found");
                throw new InvalidOperationException("Invalid username or password");
            }
            var passHash = BC.Verify(request.Password, user.PasswordHash);
            if (!passHash)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("Invalid password");
                throw new InvalidOperationException("Invalid username or password");
            }

            var token = _jwtService.GenerateToken(user);
            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Token = token,
                Role = user.Role.ToString()
            };
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Fetching user with Id: {userId}");
            var user =  await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("User not found");
                throw new NotFoundException("User not found");
            }
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"User with Id: {user.Id} found");
            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Fetching user with username: {username}");
            var user =  await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("User not found");
                throw new NotFoundException("User not found");
            }
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"User with Id: {user.Id} found");
            return user;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching all users");
            var users = await _userRepository.GetAllAsync();
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Total users found: {users.Count}");
            return users;
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Creating user with username: {request.Username}");
            var hasUsername = await _userRepository.ExistsByUsernameAsync(request.Username);
            if (hasUsername)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("Username already exists");
                throw new InvalidOperationException("Username already exists");
            }

            var hasPhone = await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber);
            if (hasPhone)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("Phone number already exists");
                throw new InvalidOperationException("Phone number already exists");
            }
            var user = new User
            {
                Username = request.Username,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BC.HashPassword(request.Password),
                Role = UserRole.User
            };

            var account = new Account
            {
                UserId = user.Id,
                BankName = "Default Bank",
                AccountNumber = $"ACC-{user.Id:N}".Substring(0, 16),
                AccountHolder = user.Username,
            };

            user.Account = account;

            await _userRepository.AddAsync(user);
            await _accountRepository.AddAsync(account);
            var changes = await _userRepository.SaveChangesAsync();

            if(changes <= 0)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("Failed to create user");
                throw new InvalidOperationException("Failed to create user");
            }
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"User with Id: {user.Id} created successfully");
            _logger.LogInformation($"Account of user with Id: {user.Id} created successfully");
            return user;
        }

        public async Task<User?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Updating user with Id: {userId}");
            var user = await _userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("User not found");
                 throw new NotFoundException("User not found");
            }

            if (!string.IsNullOrEmpty(request.Username))
            {
                if (await _userRepository.ExistsByUsernameAsync(request.Username, userId))
                {
                    _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                    _logger.LogInformation("Username already exists");
                    throw new ConflictException("Username already exists");
                }
                user.Username = request.Username;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, userId))
                {
                    _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                    _logger.LogInformation("Phone number already exists");
                    throw new ConflictException("Phone number already exists");
                }
                user.PhoneNumber = request.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BC.HashPassword(request.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"User with Id: {user.Id} updated successfully");

            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"Deleting user with Id: {userId}");
            var user = await _userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null)
            {
                _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
                _logger.LogInformation("User not found");
                 throw new NotFoundException("User not found");
            }
            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation($"User with Id: {user.Id} deleted successfully");
            return true;
        }
    }
}
