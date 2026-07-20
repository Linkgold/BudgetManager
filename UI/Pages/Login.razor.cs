using Shared.DTOs.Request;
using UI.Shared;

namespace UI.Pages
{
    public partial class Login: BasePage
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        private async Task HandleLogin()
        {
            _isLoading = true;
            _errorMessage = string.Empty;

            try
            {
                // 🔥 Enviar a la API
                bool success = await AuthService.LoginAsync(new LoginRequestDTO { Email = _email, Password = _password });

                if (success)
                {

                    NavigationManager.NavigateTo("/", true);
                }
                else
                {

                    _errorMessage = "Email o contraseña incorrectos.";
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Excepción en login para: {_email}", ex);

                _errorMessage = $"Ocurrió un error inesperado.";
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
