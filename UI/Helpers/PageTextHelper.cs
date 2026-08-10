using Microsoft.JSInterop;

namespace UI.Helpers
{
    public static class PageTextHelper
    {
        private const string DEFAULT_ENTITY_NAME = "elementos";

        // ================================================================
        // IDs DE ELEMENTOS HTML
        // ================================================================

        /// <summary>
        /// ID del input de importe por defecto en Presupuestos
        /// </summary>
        public const string BUDGET_DEFAULT_AMOUNT_ID = "budgetDefaultAmount";

        /// <summary>
        /// Prefijo para los IDs de los inputs de importe por mes.
        /// Uso: $"{PageTextHelper.BudgetMonthAmountPrefix}_{monthValue}"
        /// Ejemplo: "budgetMonthAmount_1", "budgetMonthAmount_2"
        /// </summary>
        public const string BUDGET_MONTH_AMOUNT_PREFIX = "budgetMonthAmount";

        /// <summary>
        /// ID del input de importe en Gastos Fijos
        /// </summary>
        public const string FIXED_EXPENSE_AMOUNT_ID = "fixedExpenseAmount";

        /// <summary>
        /// ID del input de importe en Transacciones
        /// </summary>
        public const string TRANSACTION_AMOUNT_ID = "transactionAmount";

        // ================================================================
        // CONFIGURACIÓN DE PÁGINAS
        // ================================================================

        private struct Definitions
        {
            public string Title;
            public string Icon;
            public string EntityName;
            public string EntityNamePlural;
        }

        private static readonly Dictionary<string, Definitions> PageConfig = new()
        {
            ["Categories"] = new Definitions { Title = "Categorías", Icon = "🏷️", EntityName = "Categoría", EntityNamePlural = "Categorías" },
            ["Transactions"] = new Definitions { Title = "Transacciones", Icon = "📒", EntityName = "Transacción", EntityNamePlural = "Transacciones" },
            ["Budgets"] = new Definitions { Title = "Presupuestos", Icon = "📊", EntityName = "Presupuesto", EntityNamePlural = "Presupuestos" },
            ["FixedExpenses"] = new Definitions { Title = "Gastos Fijos", Icon = "💸", EntityName = "Gasto Fijo", EntityNamePlural = "Gastos Fijos" },
            ["Dashboard"] = new Definitions { Title = "Resumen Anual", Icon = "📈" },
            ["MonthDetail"] = new Definitions { Title = "Resumen de", Icon = "📅" },
            ["Login"] = new Definitions { Title = "Login", Icon = "🔐" },
            ["Register"] = new Definitions { Title = "Register", Icon = "🔐" },
            ["Profile"] = new Definitions { Title = "Perfil", Icon = "👤" }
        };

        // ================================================================
        // MÉTODOS DE TEXTO
        // ================================================================

        public static string GetPageTitle(string page)
        {
            if (!PageConfig.TryGetValue(page, out Definitions config))
            {
                return page;
            }

            return $"{config.Icon} {config.Title}";
        }

        public static string GetCreateButtonText(string page)
        {
            if (!PageConfig.TryGetValue(page, out Definitions config))
            {
                return "➕ Añadir";
            }

            return $"➕ Añadir {config.EntityName}";
        }

        public static string GetEmptyMessage(string page)
        {
            return page switch
            {
                "Categories" => "No hay categorías que coincidan con los filtros.",
                "Transactions" => "No hay transacciones que coincidan con los filtros.",
                "Budgets" => "No hay presupuestos para el año seleccionado.",
                "FixedExpenses" => "No hay gastos fijos que coincidan con los filtros.",
                _ => "No hay elementos que coincidan con los filtros."
            };
        }

        public static string GetFooterMessage(int filteredCount, int totalCount, string page) => filteredCount > 0 ? $"Mostrando {filteredCount} de {totalCount} {ResolveEntityName("Budgets", filteredCount)}" : GetEmptyMessage(page);

        public static string GetBudgetFooterMessage(int filteredCount, int year) => filteredCount > 0 ? $"Mostrando {filteredCount} {ResolveEntityName("Budgets", filteredCount)} para el año {year}" : GetEmptyMessage("Budgets");

        private static string ResolveEntityName(string page, int count)
        {
            if (!PageConfig.TryGetValue(page, out Definitions config))
            {
                return DEFAULT_ENTITY_NAME;
            }

            return count > 1 ? config.EntityNamePlural.ToLower() : config.EntityName.ToLower();
        }

        // ================================================================
        // MÉTODOS DE UTILIDAD
        // ================================================================

        /// <summary>
        /// Selecciona todo el texto de un elemento HTML por su ID
        /// </summary>
        /// <param name="jsRuntime">Referencia al JSRuntime inyectado en la página</param>
        /// <param name="elementId">ID del elemento HTML a seleccionar</param>
        public static async Task SelectAllText(IJSRuntime jsRuntime, string elementId)
        {
            //await jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{elementId}').select()");
            try
            {
                await jsRuntime.InvokeVoidAsync("selectAllText", elementId);
            }
            catch (JSException ex)
            {
                // 🔍 Log para depuración
                Console.WriteLine($"❌ Error en SelectAllText: {ex.Message}");
            }
        }
    }
}