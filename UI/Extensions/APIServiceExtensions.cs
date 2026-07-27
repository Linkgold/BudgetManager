using Shared.DTOs.Request;
using Shared.DTOs.Response;
using UI.Services;

namespace UI.Extensions
{
    public static class APIServiceExtensions
    {
        // ================================================================
        // CATEGORIES
        // ================================================================

        public static async Task<List<CategoryResponseDTO>?> GetCategoriesAsync(this APIService api)
        {
            return await api.GetListAsync<CategoryResponseDTO>("/api/category");
        }

        public static async Task<CategoryResponseDTO?> CreateCategoryAsync(this APIService api, CreateCategoryRequestDTO request)
        {
            return await api.PostAsync<CreateCategoryRequestDTO, CategoryResponseDTO>("/api/category", request);
        }

        public static async Task<CategoryResponseDTO?> UpdateCategoryAsync(this APIService api, int id, UpdateCategoryRequestDTO request)
        {
            return await api.PutAsync<UpdateCategoryRequestDTO, CategoryResponseDTO>($"/api/category/{id}", request);
        }

        public static async Task<bool> DeleteCategoryAsync(this APIService api, int id)
        {
            return await api.DeleteAsync($"/api/category/{id}");
        }

        // ================================================================
        // TRANSACTIONS
        // ================================================================

        public static async Task<List<TransactionResponseDTO>?> GetTransactionsAsync(this APIService api)
        {
            return await api.GetListAsync<TransactionResponseDTO>("/api/transaction");
        }

        public static async Task<TransactionResponseDTO?> CreateTransactionAsync(this APIService api, CreateTransactionRequestDTO request)
        {
            return await api.PostAsync<CreateTransactionRequestDTO, TransactionResponseDTO>("/api/transaction", request);
        }

        public static async Task<TransactionResponseDTO?> UpdateTransactionAsync(this APIService api, int id, UpdateTransactionRequestDTO request)
        {
            return await api.PutAsync<UpdateTransactionRequestDTO, TransactionResponseDTO>($"/api/transaction/{id}", request);
        }

        public static async Task<bool> DeleteTransactionAsync(this APIService api, int id)
        {
            return await api.DeleteAsync($"/api/transaction/{id}");
        }

        // ================================================================
        // BUDGETS
        // ================================================================

        public static async Task<List<BudgetResponseDTO>?> GetBudgetsAsync(this APIService api)
        {
            return await api.GetListAsync<BudgetResponseDTO>("/api/budget");
        }

        public static async Task<BudgetResponseDTO?> CreateBudgetAsync(this APIService api, CreateBudgetRequestDTO request)
        {
            return await api.PostAsync<CreateBudgetRequestDTO, BudgetResponseDTO>("/api/budget", request);
        }

        public static async Task<BudgetResponseDTO?> UpdateBudgetAsync(this APIService api, int id, UpdateBudgetRequestDTO request)
        {
            return await api.PutAsync<UpdateBudgetRequestDTO, BudgetResponseDTO>($"/api/budget/{id}", request);
        }

        public static async Task<bool> DeleteBudgetAsync(this APIService api, int id)
        {
            return await api.DeleteAsync($"/api/budget/{id}");
        }

        public static async Task<BulkBudgetResponseDTO?> CreateBulkBudgetAsync(this APIService api, CreateBulkBudgetRequestDTO request)
        {
            return await api.PostAsync<CreateBulkBudgetRequestDTO, BulkBudgetResponseDTO>("/api/budget/bulk", request);
        }

        public static async Task<BulkBudgetResponseDTO?> UpdateBulkBudgetAsync(this APIService api, UpdateBulkBudgetRequestDTO request)
        {
            return await api.PutAsync<UpdateBulkBudgetRequestDTO, BulkBudgetResponseDTO>("/api/budget/bulk", request);
        }

        public static async Task<BulkBudgetResponseDTO?> DeleteBulkBudgetAsync(this APIService api, DeleteBulkBudgetRequestDTO request)
        {
            return await api.DeleteAsync<BulkBudgetResponseDTO>("/api/budget/bulk", request);
        }

        // ================================================================
        // FIXED EXPENSES
        // ================================================================

        public static async Task<List<FixedExpenseResponseDTO>?> GetFixedExpensesAsync(this APIService api)
        {
            return await api.GetListAsync<FixedExpenseResponseDTO>("/api/fixedexpense");
        }

        public static async Task<FixedExpenseResponseDTO?> CreateFixedExpenseAsync(this APIService api, CreateFixedExpenseRequestDTO request)
        {
            return await api.PostAsync<CreateFixedExpenseRequestDTO, FixedExpenseResponseDTO>("/api/fixedexpense", request);
        }

        public static async Task<FixedExpenseResponseDTO?> UpdateFixedExpenseAsync(this APIService api, int id, UpdateFixedExpenseRequestDTO request)
        {
            return await api.PutAsync<UpdateFixedExpenseRequestDTO, FixedExpenseResponseDTO>($"/api/fixedexpense/{id}", request);
        }

        public static async Task<bool> DeleteFixedExpenseAsync(this APIService api, int id)
        {
            return await api.DeleteAsync($"/api/fixedexpense/{id}");
        }
    }
}
