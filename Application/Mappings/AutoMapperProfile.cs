using Application.DTOs.Request;
using Application.DTOs.Response;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para mapear entre Domain y DTOs
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Domain → Response DTO
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name));

            // Request DTO → Domain (solo para el constructor)
            CreateMap<CreateCategoryRequestDto, Category>()
                .ConstructUsing(src => new Category(src.Name, src.Description));

            // Esto no es necesario porque Update usa métodos de la entidad
            // pero lo dejamos por si se necesita en otros casos
            CreateMap<UpdateCategoryRequestDto, Category>()
                .ForAllMembers(opt => opt.Ignore());
        }
    }
}
