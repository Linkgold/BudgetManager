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
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Info.Name))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Info.Description));

            // Request DTO → Domain (solo para el constructor)
            CreateMap<CreateCategoryRequestDto, Category>()
                 .ConstructUsing(src => new Category(new EntityInfo(src.Name, src.Description)));

            // Esto no es necesario porque Update usa métodos de la entidad
            // pero lo dejamos por si se necesita en otros casos
            CreateMap<UpdateCategoryRequestDto, Category>()
                .ForAllMembers(opt => opt.Ignore());

            // ==================== FIXED EXPENSE ====================

            // Domain → Response DTO
            CreateMap<FixedExpense, FixedExpenseResponseDto>()
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
            CreateMap<CreateFixedExpenseRequestDto, FixedExpense>()
                .ConstructUsing(src => new FixedExpense(
                    null, // Category se asigna después
                    new EntityInfo(src.Name, src.Description),
                    new Money(src.Amount),
                    new Period(src.Year, src.Month)
                ));

            // Update DTO → no se mapea directamente, se usa el método Update de la entidad
        }
    }
}
