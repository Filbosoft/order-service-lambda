using AutoMapper;
using Business.Commands;
using Conditus.Trader.Domain.Models;

namespace Business.MappingProfiles
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<CreateOrderCommand, Order>();
            // CreateMap<UpdateOrderCommand, Order>();
        }
    }
}