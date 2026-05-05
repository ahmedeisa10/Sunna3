using AutoMapper;
using Tamkeen.Application.DTOs.Ticket_DTOs;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;

namespace Tamkeen.Application.Mapping
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            // Ticket → TicketResponseDto
            CreateMap<Ticket, TicketResponseDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.TenantName,
                    opt => opt.MapFrom(src => src.Tenant != null
                        ? src.Tenant.FullName
                        : null))
                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor != null
                        ? src.Vendor.FullName
                        : null))
                .ForMember(dest => dest.BeforeImageUrls, opt => opt.MapFrom(src =>
        src.Images
           .Where(i => i.Type == ImageType.Before)
           .Select(i => i.Url)
           .ToList()))
    .ForMember(dest => dest.AfterImageUrls, opt => opt.MapFrom(src =>
        src.Images
           .Where(i => i.Type == ImageType.After)
           .Select(i => i.Url)
           .ToList()));

            // Image → ImageResponseDto
            CreateMap<Image, ImageResponseDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
