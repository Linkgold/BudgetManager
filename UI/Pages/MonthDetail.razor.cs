using Microsoft.AspNetCore.Components;
using System.Globalization;
using UI.Shared;

namespace UI.Pages
{
    public partial class MonthDetail : BasePage
    {
        [Parameter]
        public int Month { get; set; }

        [Parameter]
        public int Year { get; set; }

        private string _currentMonth => new DateTime(Year, Month, 1).ToString("MMMM yyyy", new CultureInfo("es-ES"));

        private List<CategoriaResumen> _categorias = new();
        private decimal _totalPresupuesto = 0;
        private decimal _totalGastos = 0;
        private decimal _totalDiferencia = 0;
        private decimal _porcentajeUtilizado = 0;

        private Dictionary<string, bool> _expandedCategories = new();
        private Dictionary<string, List<TransaccionEjemplo>> _transaccionesPorCategoria = new();

        protected override async Task OnParametersSetAsync()
        {
            if (Month == 0) Month = DateTime.Now.Month;
            if (Year == 0) Year = DateTime.Now.Year;

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // 🔥 Datos de ejemplo para categorías
            _categorias = new List<CategoriaResumen>
        {
            new CategoriaResumen { Nombre = "Alimentación", Presupuesto = 500.00m, GastosReales = 450.00m },
            new CategoriaResumen { Nombre = "Transporte", Presupuesto = 200.00m, GastosReales = 180.00m },
            new CategoriaResumen { Nombre = "Ocio", Presupuesto = 150.00m, GastosReales = 200.00m },
            new CategoriaResumen { Nombre = "Vivienda", Presupuesto = 300.00m, GastosReales = 300.00m },
            new CategoriaResumen { Nombre = "Salud", Presupuesto = 100.00m, GastosReales = 90.00m },
            new CategoriaResumen { Nombre = "Educación", Presupuesto = 80.00m, GastosReales = 0.00m },
            new CategoriaResumen { Nombre = "Otros", Presupuesto = 50.00m, GastosReales = 20.00m }
        };

            foreach (var cat in _categorias)
            {
                _expandedCategories[cat.Nombre] = false;
            }

            _transaccionesPorCategoria = new Dictionary<string, List<TransaccionEjemplo>>
            {
                ["Alimentación"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 5), Concepto = "Supermercado", Descripcion = "Compra semanal", Importe = 120.00m, Tipo = "Gasto" },
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 12), Concepto = "Panadería", Importe = 15.00m, Tipo = "Gasto" },  // Sin descripción
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 18), Concepto = "Carne", Descripcion = "Carnicería", Importe = 85.00m, Tipo = "Gasto" }
            },
                ["Transporte"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 7), Concepto = "Gasolina", Importe = 50.00m, Tipo = "Gasto" },
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 22), Concepto = "Mantenimiento", Importe = 130.00m, Tipo = "Gasto" }
            },
                ["Ocio"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 3), Concepto = "Cine", Importe = 20.00m, Tipo = "Gasto" },
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 15), Concepto = "Restaurante", Importe = 80.00m, Tipo = "Gasto" },
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 25), Concepto = "Concierto", Importe = 100.00m, Tipo = "Gasto" }
            },
                ["Vivienda"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 1), Concepto = "Alquiler", Importe = 300.00m, Tipo = "Gasto" }
            },
                ["Salud"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 10), Concepto = "Farmacia", Importe = 45.00m, Tipo = "Gasto" }
            },
                ["Educación"] = new List<TransaccionEjemplo>(),
                ["Otros"] = new List<TransaccionEjemplo>
            {
                new TransaccionEjemplo { Fecha = new DateTime(Year, Month, 8), Concepto = "Regalo", Importe = 20.00m, Tipo = "Gasto" }
            }
            };

            CalcularTotales();
            await Task.CompletedTask;
        }

        private void CalcularTotales()
        {
            _totalPresupuesto = _categorias.Sum(c => c.Presupuesto);
            _totalGastos = _categorias.Sum(c => c.GastosReales);
            _totalDiferencia = _totalPresupuesto - _totalGastos;
            _porcentajeUtilizado = _totalPresupuesto > 0 ? (_totalGastos / _totalPresupuesto) * 100 : 0;
        }

        private void ToggleExpand(string categoriaNombre)
        {
            if (_expandedCategories.ContainsKey(categoriaNombre))
            {
                _expandedCategories[categoriaNombre] = !_expandedCategories[categoriaNombre];
            }
            else
            {
                _expandedCategories[categoriaNombre] = true;
            }
        }

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", new CultureInfo("es-ES"));
        }

        private string GetStatusClass(decimal diferencia)
        {
            if (diferencia >= 9) return "green";
            if (diferencia > -10) return "yellow";
            return "red";
        }

        private async Task PreviousMonth()
        {
            if (Month == 1)
            {
                Month = 12;
                Year--;
            }
            else
            {
                Month--;
            }
            _expandedCategories.Clear();
            await LoadDataAsync();
            UpdateUrl();
        }

        private async Task NextMonth()
        {
            if (Month == 12)
            {
                Month = 1;
                Year++;
            }
            else
            {
                Month++;
            }
            _expandedCategories.Clear();
            await LoadDataAsync();
            UpdateUrl();
        }

        private void UpdateUrl()
        {
            NavigationManager.NavigateTo($"/monthly/{Month}/{Year}", replace: true);
        }

        private class CategoriaResumen
        {
            public string Nombre { get; set; } = string.Empty;
            public decimal Presupuesto { get; set; }
            public decimal GastosReales { get; set; }
            public decimal Diferencia => Presupuesto - GastosReales;
        }

        private class TransaccionEjemplo
        {
            public DateTime Fecha { get; set; }
            public string Concepto { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal Importe { get; set; }
            public string Tipo { get; set; } = "Gasto";
        }
    }
}