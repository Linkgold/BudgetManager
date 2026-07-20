using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using UI.Shared;

namespace UI.Pages
{
    public partial class Register : BasePage
    {
        private string _userName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isLoading = false;

        private async Task HandleRegister()
        {
            _isLoading = true;
            _errorMessage = string.Empty;
            _successMessage = string.Empty;

            try
            {
                // 🔥 Validaciones básicas
                if (string.IsNullOrWhiteSpace(_userName))
                {
                    _errorMessage = "El nombre de usuario es obligatorio.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(_email))
                {
                    _errorMessage = "El email es obligatorio.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(_password))
                {
                    _errorMessage = "La contraseña es obligatoria.";
                    return;
                }

                if (_password != _confirmPassword)
                {
                    _errorMessage = "Las contraseñas no coinciden.";
                    return;
                }

                if (_password.Length < 6)
                {
                    _errorMessage = "La contraseña debe tener al menos 6 caracteres.";
                    return;
                }

                // 🔥 Log de intento de registro
                await LogInfoAsync($"Intento de registro para: {_email}");

                // 🔥 Crear request
                CreateUserRequestDTO request = new CreateUserRequestDTO
                {
                    UserName = _userName,
                    Email = _email,
                    Password = _password,
                    ConfirmPassword = _confirmPassword
                };

                // 🔥 Enviar a la API
                bool success = await AuthService.RegisterAsync(request);

                if (success)
                {
                    _successMessage = "Usuario registrado correctamente. Redirigiendo al login...";

                    // 🔥 Esperar 2 segundos y redirigir al login
                    await Task.Delay(2000);
                    NavigationManager.NavigateTo("/login", true);
                }
                else
                {
                    // 🔥 Mostrar mensaje simple al usuario
                    _errorMessage = "Error al registrar el usuario";
                }
            }
            catch (Exception)
            {
                _errorMessage = $"Ocurrió un error inesperado.";
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}