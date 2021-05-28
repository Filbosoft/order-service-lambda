using AutoMapper;
using Business.Commands;
using Business.Validation.Requests;
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
                    entity => entity.OwnerId,
                    opt => opt.MapFrom(command => command.RequestingUserId)
                )
                .ForMember(
                    entity => entity.CreatedAt,
                    opt => opt.MapFrom(command => command.RequestedAt)
                )
                .ForMember(
                    entity => entity.OrderType,
                    opt => opt.MapFrom(command => command.Type)
                );
            CreateMap<OrderEntity, OrderDetail>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(entity => entity.OrderStatus)
                )
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(entity => entity.OrderType)
                );
            CreateMap<OrderEntity, OrderOverview>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(entity => entity.OrderStatus)
                )
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(entity => entity.OrderType)
                );

            CreateMap<UpdateOrderCommand, ValidateUpdateOrderRequest>();
        }
    }
}