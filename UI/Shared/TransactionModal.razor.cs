using Contracts.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs.Request;
using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Forms;
using UI.Services.API;
using UI.Services.Interfaces;

namespace UI.Shared
{
    public partial class TransactionModal : ComponentBase
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private ILogService LogService { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public EventCallback OnTransactionSaved { get; set; }

        [Parameter]
        public EventCallback OnTransactionCancelled { get; set; }

        [Parameter]
        public List<CategoryModel> Categories { get; set; } = new();

        [Parameter]
        public TransactionModel? TransactionReceived { get; set; }

        [Parameter]
        public int DefaultYear { get; set; } = DateTime.Now.Year;

        [Parameter]
        public int DefaultMonth { get; set; } = DateTime.Now.Month;

        [Parameter]
        public FormModeEnum Mode { get; set; } = FormModeEnum.Create;

        private TransactionFormModel _transactionForm = new();

        private string _suggestedName = "Ej. Compra supermercado";

        private bool ShowTypeSelector =>
            !_transactionForm.IsDeleting &&
            _transactionForm.CategoryId > 0 &&
            _transactionForm.CategoryNature == CategoryNatureEnum.Mixed;

        protected override void OnParametersSet()
        {
            if (IsVisible)
            {
                if (TransactionReceived != null)
                {
                    FillFormFromModel(TransactionReceived, Mode);
                }
            }
        }

        private void FillFormFromModel(TransactionModel model, FormModeEnum mode)
        {
            CategoryModel? category = Categories.FirstOrDefault(c => c.Id == model.CategoryId);

            TransactionTypeEnum transactionType = mode == FormModeEnum.Edit
                ? model.TransactionType
                : category?.Nature.GetDefaultTransactionTypeForCategory() ?? TransactionTypeEnum.Expense;

            if(string.IsNullOrEmpty(model.Name))
            {
                _suggestedName = category != null ? $"Ej. {category.Name}" : _suggestedName;
            }

            _transactionForm = new TransactionFormModel
            {
                Id = model.Id,
                CategoryId = model.CategoryId,
                CategoryName = model.CategoryName,
                CategoryNature = category?.Nature ?? CategoryNatureEnum.Expense,
                Name = model.Name ?? string.Empty,
                Description = model.Description ?? string.Empty,
                Amount = Math.Abs(model.Amount),
                OriginalYear = model.Date.Year > 1 ? model.Date.Year : DefaultYear,
                Date = model.Date == DateTime.MinValue ? MonthHelper.GetDefaultDate(DefaultMonth, DefaultYear) : model.Date,
                TransactionType = transactionType,
                IsModalOpen = true,
                IsEditing = mode == FormModeEnum.Edit,
                IsDeleting = mode == FormModeEnum.Delete
            };
        }

        private void OnCategoryChanged()
        {
            CategoryModel? category = Categories.FirstOrDefault(c => c.Id == _transactionForm.CategoryId);

            if (category != null)
            {
                _transactionForm.CategoryNature = category.Nature;
                _transactionForm.TransactionType = category.Nature.GetDefaultTransactionTypeForCategory();

                _suggestedName = $"Ej. {category.Name}";
            }

            StateHasChanged();
        }

        private async Task HandleSave()
        {
            try
            {
                bool success = false;

                if (_transactionForm.IsDeleting)
                {
                    success = await DeleteTransactionAsync();
                }
                else if (_transactionForm.IsEditing)
                {
                    success = await UpdateTransactionAsync();
                }
                else
                {
                    success = await CreateTransactionAsync();
                }

                if (success)
                {
                    _transactionForm.IsModalOpen = false;
                    await OnTransactionSaved.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al guardar la transacción", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> CreateTransactionAsync()
        {
            if (string.IsNullOrWhiteSpace(_transactionForm.Name))
            {
                CategoryModel? category = Categories.FirstOrDefault(c => c.Id == _transactionForm.CategoryId);
                _transactionForm.Name = category?.Name ?? "Sin nombre";
            }

            CreateTransactionRequestDTO request = new()
            {
                CategoryId = _transactionForm.CategoryId,
                Name = _transactionForm.Name,
                Description = _transactionForm.Description,
                Amount = _transactionForm.Amount,
                TransactionType = _transactionForm.TransactionType,
                Date = _transactionForm.Date
            };

            TransactionModel? result = await APIService.CreateTransactionAsync(request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] creada correctamente.");
                return true;
            }

            return false;
        }

        private async Task<bool> UpdateTransactionAsync()
        {
            UpdateTransactionRequestDTO request = new()
            {
                CategoryId = _transactionForm.CategoryId,
                Name = _transactionForm.Name,
                Description = _transactionForm.Description,
                Amount = _transactionForm.Amount,
                TransactionType = _transactionForm.TransactionType,
                Date = _transactionForm.Date
            };

            TransactionModel? result = await APIService.UpdateTransactionAsync(_transactionForm.Id, request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] actualizada correctamente.");
                return true;
            }

            return false;
        }

        private async Task<bool> DeleteTransactionAsync()
        {
            bool success = await APIService.DeleteTransactionAsync(_transactionForm.Id, _transactionForm.Name);

            if (success)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] eliminada correctamente.");
                return true;
            }

            return false;
        }

        private async Task HandleClose()
        {
            _transactionForm.IsModalOpen = false;
            _suggestedName = "Ej. Compra supermercado";
            await OnTransactionCancelled.InvokeAsync();
        }
    }
}