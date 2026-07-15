namespace Contracts.Enums
{
    /// <summary>
    /// Representa el estado de un presupuesto en relación con los gastos reales
    /// </summary>
    public enum BudgetStatusEnum
    {
        /// <summary>
        /// El presupuesto está dentro de lo esperado (gastos < 80%)
        /// </summary>
        Green = 1,
        /// <summary>
        /// El presupuesto está cerca del límite (gastos entre 80% y 100%)
        /// </summary>
        Yellow = 2,
        /// <summary>
        /// El presupuesto se ha excedido (gastos > 100%)
        /// </summary>
        Red = 3
    }
}