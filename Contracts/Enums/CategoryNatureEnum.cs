namespace Contracts.Enums
{
    /// <summary>
    /// Define la naturaleza de una categoría
    /// </summary>
    public enum CategoryNatureEnum
    {
        /// <summary>
        /// Solo permite gastos (transacciones positivas)
        /// </summary>
        Expense = 1,

        /// <summary>
        /// Solo permite ingresos (transacciones positivas)
        /// </summary>
        Income = 2,

        /// <summary>
        /// Permite tanto ingresos como gastos (transacciones positivas o negativas)
        /// </summary>
        Mixed = 3
    }
}