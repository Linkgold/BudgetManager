using Application.DTOs.Request;
using Application.DTOs.Response;
using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para mapear entre Domain y DTOs
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ==================== CATEGORY ====================

            // Domain → Response DTO
            CreateMap<Category, CategoryResponseDTO>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Info.Name))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Info.Description));

            // Request DTO → Domain (solo para el constructor)
            CreateMap<CreateCategoryRequestDTO, Category>()
                 .ConstructUsing(src => new Category(new EntityInfo(src.Name, src.Description)));

            // Esto no es necesario porque Update usa métodos de la entidad
            // pero lo dejamos por si se necesita en otros casos
            CreateMap<UpdateCategoryRequestDTO, Category>()
                .ForAllMembers(opt => opt.Ignore());

            // ==================== FIXED EXPENSE ====================

            // Domain → Response DTO
            CreateMap<FixedExpense, FixedExpenseResponseDTO>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Info.Name))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Info.Description))
                .ForMember(dest => dest.Amount,
                    opt => opt.MapFrom(src => src.Amount.Value))
                .ForMember(dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Amount.Currency))
                .ForMember(dest => dest.Year,
                    opt => opt.MapFrom(src => src.ChargePeriod.Year))
                .ForMember(dest => dest.Month,
                    opt => opt.MapFrom(src => src.ChargePeriod.Month))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Info.Name : string.Empty));

            // Request DTO → Domain
            CreateMap<CreateFixedExpenseRequestDTO, FixedExpense>()
                .ConstructUsing(src => new FixedExpense(
                    null, // Category se asigna después
                    new EntityInfo(src.Name, src.Description),
                    new Money(src.Amount),
                    new MonthlyPeriod(src.Month, src.Year)
                ));

            // ==================== BUDGET ====================

            // Domain → Response DTO
            CreateMap<Budget, BudgetResponseDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Info.Name : string.Empty))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Period.Year))
                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Period.Month))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.MonthlyAmount.Value))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.MonthlyAmount.Currency));

            // Request DTO → Domain
            CreateMap<CreateBudgetRequestDTO, Budget>()
                .ConstructUsing(src => new Budget(
                    null, // Category se asigna en el servicio
                    new Money(src.Amount),
                    new MonthlyPeriod(src.Month, src.Year)));

            // Update DTO → no se mapea directamente, se usa el método Update de la entidad

            // ==================== TRANSACTION ====================

            // Domain → Response
            CreateMap<Transaction, TransactionResponseDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Info.Name : string.Empty))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Value))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToDateTime()));

            // Request → Domain
            CreateMap<CreateTransactionRequestDTO, Transaction>()
                .ConstructUsing
                (
                    src => new Transaction
                    (
                    null, // Category se asigna en el servicio
                    new EntityInfo(src.Name, src.Description),
                    new Money(src.Amount),
                    src.Type,
                    new DailyPeriod(src.Date.Day, src.Date.Month, src.Date.Year)
                    )
                );
        }
    }
}