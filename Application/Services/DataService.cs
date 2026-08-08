using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using Shared.DTOs.Response.MonthDetail;

namespace Application.Services.Data
{
    public class DataService : IDataService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFixedExpenseRepository _fixedExpenseRepository;
        private readonly IMapper _mapper;

        public DataService(
            ICurrentUserService currentUserService,
            ICategoryRepository categoryRepository,
            IBudgetRepository budgetRepository,
            ITransactionRepository transactionRepository,
            IFixedExpenseRepository fixedExpenseRepository,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
            _fixedExpenseRepository = fixedExpenseRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ================================================================
        // DASHBOARD
        // ================================================================

        public async Task<DashboardResponseDTO> GetDashboardDataAsync(int year)
        {
            DashboardResponseDTO dashboard = new DashboardResponseDTO();

            // Obtener datos
            IEnumerable<Category> categories = await _categoryRepository.GetAllAsync(UserId);
            //IEnumerable<Budget> budgets = await _budgetRepository.GetByPeriodAsync(UserId, new MonthlyPeriod(1, year));
            // TODO: Obtener transacciones del año
            // TODO: Obtener gastos fijos del año

            // Procesar categorías
            // Procesar meses
            // Calcular totales

            return dashboard;
        }

        // ================================================================
        // MONTH DETAIL
        // ================================================================

        public async Task<MonthDetailResponseDTO> GetMonthDetailAsync(int year, int month)
        {
            MonthDetailResponseDTO monthDetail = new MonthDetailResponseDTO();

            // TODO: Obtener datos del mes

            return monthDetail;
        }

        // ================================================================
        // HAS DATA
        // ================================================================

        public async Task<HasDataResponseDTO> GetHasDataAsync()
        {
            HasDataResponseDTO hasData = new HasDataResponseDTO();

            // TODO: Obtener años con datos

            return hasData;
        }
    }
}