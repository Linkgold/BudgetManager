using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar usuarios y autenticación
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(userService);
            _userService = userService;
        }

        // ==================== AUTENTICACIÓN (público) ====================

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            UserResponseDTO createdUser = await _userService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Inicia sesión y devuelve un token JWT
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            LoginResponseDTO token = await _userService.LoginAsync(request.Email, request.Password);

            return Ok(token);
        }

        // ==================== USUARIOS (requiere autenticación) ====================

        /// <summary>
        /// Obtiene el usuario autenticado actual
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            UserResponseDTO user = await _userService.GetCurrentUserAsync();

            return Ok(user);
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid user ID");

            UserResponseDTO user = await _userService.GetByIdAsync(id);

            return Ok(user);
        }

        /// <summary>
        /// Actualiza el usuario autenticado actual
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            UserResponseDTO updatedUser = await _userService.UpdateAsync(request);

            return Ok(updatedUser);
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            await _userService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword);

            return NoContent();
        }

        /// <summary>
        /// Elimina al usuario autenticado
        /// </summary>
        [HttpDelete("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            await _userService.DeleteAsync();

            return NoContent();
        }
    }
}