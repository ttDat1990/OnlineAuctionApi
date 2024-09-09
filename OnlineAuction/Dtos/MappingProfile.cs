namespace OnlineAuction.Dtos;
using AutoMapper;
using OnlineAuction.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Category, CategoryDto>().ReverseMap();
    }
}