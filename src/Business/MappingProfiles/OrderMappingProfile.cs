using AutoMapper;
using Business.Commands;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Models;

namespace Business.MappingProfiles
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<CreateOrderCommand, OrderEntity>()
                .ForMember(
                    entity => entity.CreatedBy,
                    opt => opt.MapFrom(command => command.RequestingUserId)
                )
                .ForMember(
                    entity => entity.CreatedAt,
                    opt => opt.MapFrom(command => command.RequestedAt)
                );
            CreateMap<OrderEntity, OrderDetail>();
            CreateMap<OrderEntity, OrderOverview>();
        }
    }
}