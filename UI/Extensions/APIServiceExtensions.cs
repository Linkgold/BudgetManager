using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using UI.Extensions.Mappings;
using UI.Models;
using UI.Models.Dashboard;
using UI.Services.API;

namespace UI.Extensions
{
    public static class APIServiceExtensions
    {
        // ================================================================
        // CATEGORIES
        // ================================================================

        public static async Task<List<CategoryModel>?> GetCategoriesAsync(this APIService api)
        {
            APIResult<List<CategoryResponseDTO>> result = await api.GetListAsync<CategoryResponseDTO>("/api/category");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToCategoryModelList();
            }

            string message = result.ErrorMessage ?? "Error al cargar las categorías.";

            api.NotifyError(message);

            return null;
        }

        public static async Task<CategoryModel?> CreateCategoryAsync(this APIService api, CreateCategoryRequestDTO request)
        {
            APIResult<CategoryResponseDTO> result = await api.PostAsync<CreateCategoryRequestDTO, CategoryResponseDTO>("/api/category", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Categoría [{request.Name}] creada correctamente.");

                return result.Data.ToCategoryModel();
            }

            string message = result.ErrorMessage ?? "Error al crear la categoría.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<CategoryModel?> UpdateCategoryAsync(this APIService api, int id, UpdateCategoryRequestDTO request)
        {
            APIResult<CategoryResponseDTO> result = await api.PutAsync<UpdateCategoryRequestDTO, CategoryResponseDTO>($"/api/category/{id}", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Categoría [{request.Name}] actualizada correctamente.");

                return result.Data.ToCategoryModel();
            }

            string message = result.ErrorMessage ?? "Error al actualizar la categoría.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<bool> DeleteCategoryAsync(this APIService api, int id, string categoryName)
        {
            APIResult<bool> result = await api.DeleteAsync($"/api/category/{id}");

            if (result.IsSuccess)
            {
                api.NotifySuccess($"Categoría [{categoryName}] eliminada correctamente.");

                return true;
            }

            string message = result.ErrorMessage ?? "Error al eliminar la categoría.";
            api.NotifyError(message);

            return false;
        }

        // ================================================================
        // TRANSACTIONS
        // ================================================================

        public static async Task<List<TransactionModel>?> GetTransactionsAsync(this APIService api, int year)
        {
            APIResult<List<TransactionResponseDTO>> result = await api.GetListAsync<TransactionResponseDTO>($"/api/transaction/year/{year}");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToTransactionModelList();
            }

            string message = result.ErrorMessage ?? "Error al cargar las transacciones.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<TransactionModel?> CreateTransactionAsync(this APIService api, CreateTransactionRequestDTO request)
        {
            APIResult<TransactionResponseDTO> result = await api.PostAsync<CreateTransactionRequestDTO, TransactionResponseDTO>("/api/transaction", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Transacción [{request.Name}] creada correctamente.");
                return result.Data.ToTransactionModel();
            }

            string message = result.ErrorMessage ?? "Error al crear la transacción.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<TransactionModel?> UpdateTransactionAsync(this APIService api, int id, UpdateTransactionRequestDTO request)
        {
            APIResult<TransactionResponseDTO> result = await api.PutAsync<UpdateTransactionRequestDTO, TransactionResponseDTO>($"/api/transaction/{id}", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Transacción [{request.Name}] actualizada correctamente.");
                return result.Data.ToTransactionModel();
            }

            string message = result.ErrorMessage ?? "Error al actualizar la transacción.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<bool> DeleteTransactionAsync(this APIService api, int id, string name)
        {
            APIResult<bool> result = await api.DeleteAsync($"/api/transaction/{id}");

            if (result.IsSuccess)
            {
                api.NotifySuccess($"Transacción [{name}] eliminada correctamente.");
                return true;
            }

            string message = result.ErrorMessage ?? "Error al eliminar la transacción.";
            api.NotifyError(message);

            return false;
        }

        // ================================================================
        // BUDGETS
        // ================================================================

        public static async Task<List<BudgetModel>?> GetBudgetsAsync(this APIService api, int year)
        {
            APIResult<List<BudgetResponseDTO>> result = await api.GetListAsync<BudgetResponseDTO>($"/api/budget/year/{year}");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToBudgetModelList();
            }

            string message = result.ErrorMessage ?? "Error al cargar los presupuestos.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<BulkBudgetModel?> CreateBulkBudgetAsync(this APIService api, CreateBulkBudgetRequestDTO request)
        {
            APIResult<BulkBudgetResponseDTO> result = await api.PostAsync<CreateBulkBudgetRequestDTO, BulkBudgetResponseDTO>("/api/budget/bulk", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Presupuestos creados correctamente para {request.Year}.");
                return result.Data.ToBulkBudgetModel();
            }

            string message = result.ErrorMessage ?? "Error al crear los presupuestos.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<BulkBudgetModel?> UpdateBulkBudgetAsync(this APIService api, UpdateBulkBudgetRequestDTO request)
        {
            APIResult<BulkBudgetResponseDTO> result = await api.PutAsync<UpdateBulkBudgetRequestDTO, BulkBudgetResponseDTO>("/api/budget/bulk", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Presupuestos actualizados correctamente para {request.Year}.");
                return result.Data.ToBulkBudgetModel();
            }

            string message = result.ErrorMessage ?? "Error al actualizar los presupuestos.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<BulkBudgetModel?> DeleteBulkBudgetAsync(this APIService api, DeleteBulkBudgetRequestDTO request)
        {
            APIResult<BulkBudgetResponseDTO> result = await api.DeleteAsync<BulkBudgetResponseDTO>("/api/budget/bulk", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Presupuestos eliminados correctamente.");
                return result.Data.ToBulkBudgetModel();
            }

            string message = result.ErrorMessage ?? "Error al eliminar los presupuestos.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<BudgetModel?> UpdateBudgetAsync(this APIService api, int id, UpdateBudgetRequestDTO request)
        {
            APIResult<BudgetResponseDTO> result = await api.PutAsync<UpdateBudgetRequestDTO, BudgetResponseDTO>($"/api/budget/{id}", request);

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToBudgetModel();
            }

            string message = result.ErrorMessage ?? "Error al actualizar el presupuesto.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<bool> DeleteBudgetAsync(this APIService api, int id)
        {
            APIResult<bool> result = await api.DeleteAsync($"/api/budget/{id}");

            if (result.IsSuccess)
            {
                return true;
            }

            string message = result.ErrorMessage ?? "Error al eliminar el presupuesto.";
            api.NotifyError(message);

            return false;
        }

        // ================================================================
        // FIXED EXPENSES
        // ================================================================

        public static async Task<List<FixedExpenseModel>?> GetFixedExpensesAsync(this APIService api, int year)
        {
            APIResult<List<FixedExpenseResponseDTO>> result = await api.GetListAsync<FixedExpenseResponseDTO>($"/api/fixedexpense/year/{year}");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToFixedExpenseModelList();
            }

            string message = result.ErrorMessage ?? "Error al cargar los gastos fijos.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<FixedExpenseModel?> CreateFixedExpenseAsync(this APIService api, CreateFixedExpenseRequestDTO request)
        {
            APIResult<FixedExpenseResponseDTO> result = await api.PostAsync<CreateFixedExpenseRequestDTO, FixedExpenseResponseDTO>("/api/fixedexpense", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Gasto fijo [{request.Name}] creado correctamente.");
                return result.Data.ToFixedExpenseModel();
            }

            string message = result.ErrorMessage ?? "Error al crear el gasto fijo.";
            api.NotifyError(message);
            return null;
        }

        public static async Task<FixedExpenseModel?> UpdateFixedExpenseAsync(this APIService api, int id, UpdateFixedExpenseRequestDTO request)
        {
            APIResult<FixedExpenseResponseDTO> result = await api.PutAsync<UpdateFixedExpenseRequestDTO, FixedExpenseResponseDTO>($"/api/fixedexpense/{id}", request);

            if (result.IsSuccess && result.Data != null)
            {
                api.NotifySuccess($"Gasto fijo [{request.Name}] actualizado correctamente.");

                return result.Data.ToFixedExpenseModel();
            }

            string message = result.ErrorMessage ?? "Error al actualizar el gasto fijo.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<bool> DeleteFixedExpenseAsync(this APIService api, int id, string name)
        {
            APIResult<bool> result = await api.DeleteAsync($"/api/fixedexpense/{id}");

            if (result.IsSuccess)
            {
                api.NotifySuccess($"Gasto fijo [{name}] eliminado correctamente.");

                return true;
            }

            string message = result.ErrorMessage ?? "Error al eliminar el gasto fijo.";
            api.NotifyError(message);

            return false;
        }

        // ================================================================
        // DATA
        // ================================================================

        public static async Task<DashboardModel?> GetDashboardDataAsync(this APIService api, int year)
        {
            APIResult<DashboardResponseDTO> result = await api.GetAsync<DashboardResponseDTO>($"/api/data/dashboard/{year}");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToDashboardModel();
            }

            string message = result.ErrorMessage ?? "Error al cargar los datos del dashboard.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<HasDataModel?> GetHasDataAsync(this APIService api)
        {
            APIResult<HasDataResponseDTO> result = await api.GetAsync<HasDataResponseDTO>("/api/data/has-data");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.ToHasDataModel();
            }

            string message = result.ErrorMessage ?? "Error al cargar la información de datos existentes.";
            api.NotifyError(message);

            return null;
        }

        // ================================================================
        // USER
        // ================================================================

        public static async Task<UserResponseDTO?> GetCurrentUserAsync(this APIService api)
        {
            APIResult<UserResponseDTO> result = await api.GetAsync<UserResponseDTO>("/api/user/me");

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data;
            }

            string message = result.ErrorMessage ?? "Error al cargar los datos del usuario.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<UserResponseDTO?> UpdateUserAsync(this APIService api, UpdateUserRequestDTO request)
        {
            APIResult<UserResponseDTO> result = await api.PutAsync<UpdateUserRequestDTO, UserResponseDTO>("/api/user/me", request);

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data;
            }

            string message = result.ErrorMessage ?? "Error al actualizar el perfil.";
            api.NotifyError(message);

            return null;
        }

        public static async Task<bool> ChangePasswordAsync(this APIService api, ChangePasswordRequestDTO request)
        {
            APIResult<bool> result = await api.PostNoContentAsync("/api/user/change-password", request);

            if (result.IsSuccess)
            {
                return true;
            }

            if (result.StatusCode == 403)
            {
                api.NotifyError("La contraseña actual es incorrecta.");
                return false;
            }

            string message = result.ErrorMessage ?? "Error al cambiar la contraseña.";
            api.NotifyError(message);

            return false;
        }

        public static async Task<bool> DeleteUserAsync(this APIService api)
        {
            APIResult<bool> result = await api.DeleteAsync("/api/user/me");

            if (result.IsSuccess)
            {
                return true;
            }

            string message = result.ErrorMessage ?? "Error al eliminar la cuenta.";
            api.NotifyError(message);

            return false;
        }
    }
}