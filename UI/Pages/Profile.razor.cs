using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using UI.Extensions;
using UI.Services;
using UI.Services.API;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class Profile : BasePage
    {
        [Inject]
        private IStorageService StorageService { get; set; } = default!;


        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        // ================================================================
        // 1. MODELOS Y ESTADO
        // ================================================================

        private UserResponseDTO? _user;
        private string _userName = string.Empty;
        private string _email = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        private bool _isLoading = true;

        private string _originalUserName = string.Empty;
        private string _originalEmail = string.Empty;

        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;

        private bool _isDeleteModalOpen = false;

        // ================================================================
        // 2. PROPIEDADES CALCULADAS
        // ================================================================

        private bool HasChanges => _userName != _originalUserName || _email != _originalEmail;

        private bool CanChangePassword =>
            !string.IsNullOrEmpty(_currentPassword) &&
            !string.IsNullOrEmpty(_newPassword) &&
            !string.IsNullOrEmpty(_confirmNewPassword) &&
            _newPassword == _confirmNewPassword &&
            _newPassword.Length >= 6;

        private bool PasswordsMatch =>
            !string.IsNullOrEmpty(_newPassword) &&
            !string.IsNullOrEmpty(_confirmNewPassword) &&
            _newPassword == _confirmNewPassword;

        // ================================================================
        // 3. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadUserData();
        }

        // ================================================================
        // 4. CARGA DE DATOS
        // ================================================================

        private async Task LoadUserData()
        {
            try
            {
                _isLoading = true;

                UserResponseDTO? user = await APIService.GetCurrentUserAsync();

                if (user != null)
                {
                    _user = user;
                    _userName = user.UserName;
                    _email = user.Email;
                    _createdAt = user.CreatedAt;

                    _originalUserName = _userName;
                    _originalEmail = _email;
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al cargar los datos del usuario", ex);

                ToastService.ShowError("Error al cargar los datos del perfil.");
            }
            finally
            {
                _isLoading = false;
            }
        }

        // ================================================================
        // 5. ACTUALIZAR PERFIL
        // ================================================================

        private async Task SaveProfile()
        {
            if (!HasChanges)
            {
                ToastService.ShowWarning("No hay cambios para guardar.");

                return;
            }

            try
            {
                UpdateUserRequestDTO request = new()
                {
                    UserName = _userName,
                    Email = _email
                };

                UserResponseDTO? result = await APIService.UpdateUserAsync(request);

                if (result != null)
                {
                    _originalUserName = _userName;
                    _originalEmail = _email;

                    await StorageService.SetItemAsync("user_name", _userName);
                    await StorageService.SetItemAsync("user_email", _email);

                    if (AuthenticationStateProvider is CustomAuthenticationStateProvider customProvider)
                    {
                        await customProvider.RefreshUserAsync();
                    }

                    ToastService.ShowSuccess("Perfil actualizado correctamente.");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al actualizar el perfil", ex);

                ToastService.ShowError("Error al actualizar el perfil.");
            }
        }

        private void CancelChanges()
        {
            _userName = _originalUserName;
            _email = _originalEmail;

            StateHasChanged();
        }

        // ================================================================
        // 6. CAMBIAR CONTRASEÑA
        // ================================================================

        private async Task ChangePassword()
        {
            if (!CanChangePassword)
            {
                ToastService.ShowWarning("Revisa que las contraseñas coincidan y tengan al menos 6 caracteres.");

                return;
            }

            try
            {
                ChangePasswordRequestDTO request = new()
                {
                    CurrentPassword = _currentPassword,
                    NewPassword = _newPassword,
                    ConfirmNewPassword = _confirmNewPassword
                };

                bool success = await APIService.ChangePasswordAsync(request);

                if (success)
                {
                    _currentPassword = string.Empty;
                    _newPassword = string.Empty;
                    _confirmNewPassword = string.Empty;

                    ToastService.ShowSuccess("Contraseña cambiada correctamente.");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al cambiar la contraseña", ex);

                ToastService.ShowError("Error al cambiar la contraseña.");
            }
        }

        // ================================================================
        // 7. ELIMINAR CUENTA
        // ================================================================

        private void OpenDeleteConfirmation()
        {
            _isDeleteModalOpen = true;

            StateHasChanged();
        }

        private async Task ConfirmDelete()
        {
            try
            {
                bool success = await APIService.DeleteUserAsync();

                if (success)
                {
                    await APIService.LogoutAsync();
                    NavigationManager.NavigateTo("/login", true);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al eliminar la cuenta", ex);
                ToastService.ShowError("Error al eliminar la cuenta.");
            }
            finally
            {
                _isDeleteModalOpen = false;
            }
        }

        private void CancelDelete()
        {
            _isDeleteModalOpen = false;

            StateHasChanged();
        }
    }
}