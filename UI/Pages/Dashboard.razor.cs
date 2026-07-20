using System.Globalization;
using UI.Shared;

namespace UI.Pages
{
    public partial class Dashboard: BasePage
    {
        private int _currentYear = DateTime.Now.Year;

        private List<string> _categorias = new()
    {
        "Alimentación",
        "Transporte",
        "Ocio",
        "Vivienda",
        "Salud",
        "Educación",
        "Otros"
    };

        private List<MesData> _meses = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            _meses = new List<MesData>
        {
            new MesData("Enero", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>
            {
                { "Alimentación", (500.00m, 450.00m) },
                { "Transporte", (200.00m, 180.00m) },
                { "Ocio", (150.00m, 200.00m) },
                { "Vivienda", (300.00m, 300.00m) },
                { "Salud", (100.00m, 90.00m) },
                { "Educación", (80.00m, 0.00m) },
                { "Otros", (50.00m, 20.00m) }
            }),

            new MesData("Febrero", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>
            {
                { "Alimentación", (500.00m, 480.00m) },
                { "Transporte", (200.00m, 200.00m) },
                { "Ocio", (150.00m, 150.00m) },
                { "Vivienda", (300.00m, 300.00m) },
                { "Salud", (100.00m, 90.00m) },
                { "Educación", (80.00m, 0.00m) },
                { "Otros", (50.00m, 30.00m) }
            }),

            new MesData("Marzo", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>
            {
                { "Alimentación", (500.00m, 520.00m) },
                { "Transporte", (200.00m, 220.00m) },
                { "Ocio", (150.00m, 180.00m) },
                { "Vivienda", (300.00m, 300.00m) },
                { "Salud", (100.00m, 110.00m) },
                { "Educación", (80.00m, 80.00m) },
                { "Otros", (50.00m, 40.00m) }
            }),

            new MesData("Abril", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>
            {
                { "Alimentación", (500.00m, 490.00m) },
                { "Transporte", (200.00m, 190.00m) },
                { "Ocio", (150.00m, 160.00m) },
                { "Vivienda", (300.00m, 300.00m) },
                { "Salud", (100.00m, 95.00m) },
                { "Educación", (80.00m, 0.00m) },
                { "Otros", (50.00m, 40.00m) }
            }),

            new MesData("Mayo", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Junio", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Julio", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Agosto", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Septiembre", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Octubre", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Noviembre", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>()),
            new MesData("Diciembre", new Dictionary<string, (decimal Presupuesto, decimal Gasto)>())
        };

            await Task.CompletedTask;
        }

        private void NavigateToMonth(string monthName)
        {
            try
            {
                var monthNumber = DateTime.ParseExact(monthName, "MMMM", new CultureInfo("es-ES")).Month;
                NavigationManager.NavigateTo($"/monthly/{monthNumber}/{_currentYear}");
            }
            catch (Exception ex)
            {
                // Fallback: si falla, ir al mes actual
                Console.WriteLine($"Error navegando a {monthName}: {ex.Message}");
                NavigationManager.NavigateTo("/monthly");
            }
        }

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", new CultureInfo("es-ES"));
        }

        private class MesData
        {
            public string Nombre { get; }
            private Dictionary<string, (decimal Presupuesto, decimal Gasto)> _valores { get; }

            public MesData(string nombre, Dictionary<string, (decimal Presupuesto, decimal Gasto)> valores)
            {
                Nombre = nombre;
                _valores = valores ?? new Dictionary<string, (decimal Presupuesto, decimal Gasto)>();
            }

            public (decimal Presupuesto, decimal Gasto, decimal Diferencia, bool TieneDatos) ObtenerValor(string categoria)
            {
                if (_valores.TryGetValue(categoria, out var valor))
                {
                    return (valor.Presupuesto, valor.Gasto, valor.Presupuesto - valor.Gasto, true);
                }
                return (0, 0, 0, false);
            }
        }
    }
}
