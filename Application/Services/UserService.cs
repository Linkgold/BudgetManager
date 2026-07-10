using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Crypt = BCrypt.Net.BCrypt;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UserService(IUserRepository userRepository, IMapper mapper, ICurrentUserService currentUserService)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(currentUserService);

            _userRepository = userRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        // ==================== CONSULTAS ====================

        public async Task<UserResponseDTO> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid user ID", nameof(id));

            User? user = await _userRepository.GetByIdAsync(id);

            if (user == null) throw new KeyNotFoundException($"User with ID {id} not found");

            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));

            User? user = await _userRepository.GetByEmailAsync(email);

            if (user == null) throw new KeyNotFoundException($"User with email {email} not found");

            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO> GetCurrentUserAsync()
        {
            int currentUserId = _currentUserService.UserId;

            if (currentUserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            return await GetByIdAsync(currentUserId);
        }

        // ==================== COMANDOS ====================

        public async Task<UserResponseDTO> CreateAsync(CreateUserRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (await _userRepository.ExistsByEmailAsync(request.Email)) throw new InvalidOperationException($"User with email {request.Email} already exists");

            // Crear Value Objects
            UserInfo userInfo = new UserInfo(request.UserName, request.Email);
            string passwordHash = Crypt.HashPassword(request.Password);

            // Crear usuario
            User user = new User(userInfo, passwordHash);
            await _userRepository.AddAsync(user);

            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<UserResponseDTO> UpdateAsync(UpdateUserRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            int currentUserId = _currentUserService.UserId;

            // Obtener usuario existente
            User? user = await _userRepository.GetByIdAsync(currentUserId);

            if (user == null) throw new KeyNotFoundException($"User with ID {currentUserId} not found");

            // Verificar que el email no esté siendo usado por otro usuario
            User? existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null && existingUser.Id != currentUserId) throw new InvalidOperationException($"Email {request.Email} is already in use by another user");

            // Actualizar
            UserInfo newUserInfo = new UserInfo(request.UserName, request.Email);
            user.Update(newUserInfo);

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task DeleteAsync()
        {
            int currentUserId = _currentUserService.UserId;

            if (!await _userRepository.ExistsAsync(currentUserId)) throw new KeyNotFoundException($"User with ID {currentUserId} not found");

            await _userRepository.DeleteAsync(currentUserId);
        }

        // ==================== AUTENTICACIÓN ====================

        public async Task<string> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty", nameof(password));

            // Obtener usuario por email
            User user = await _userRepository.GetByEmailAsync(email);

            if (user == null) throw new UnauthorizedAccessException("Invalid email or password");

            // Verificar si el usuario está activo
            if (!user.IsActive) throw new UnauthorizedAccessException("User account is disabled");

            // Verificar contraseña
            if (!Crypt.Verify(password, user.PasswordHash)) throw new UnauthorizedAccessException("Invalid email or password");

            // Aquí iría la generación del token JWT
            // Por ahora, devolvemos un token dummy
            return $"token_{user.Id}_{user.Info.UserName}";
        }

        public async Task<bool> ValidatePasswordAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return false;

            User user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !user.IsActive) return false;

            return Crypt.Verify(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword)) throw new ArgumentException("Current password cannot be empty", nameof(currentPassword));
            if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentException("New password cannot be empty", nameof(newPassword));
            if (newPassword.Length < 6) throw new ArgumentException("New password must have at least 6 characters", nameof(newPassword));

            int userId = _currentUserService.UserId;

            // Obtener usuario
            User? user = await _userRepository.GetByIdAsync(userId);

            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

            // Verificar contraseña actual
            if (!Crypt.Verify(currentPassword, user.PasswordHash)) throw new UnauthorizedAccessException("Current password is incorrect");

            // Actualizar contraseña
            string newPasswordHash = Crypt.HashPassword(newPassword);
            user.UpdatePassword(newPasswordHash);

            await _userRepository.UpdateAsync(user);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            return await _userRepository.ExistsAsync(id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            return await _userRepository.ExistsByEmailAsync(email);
        }
    }
}