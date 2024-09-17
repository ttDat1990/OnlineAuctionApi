namespace OnlineAuction.Dtos;
using AutoMapper;
using OnlineAuction.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<CreateItemWithFilesDto, Item>()
            .ForMember(dest => dest.CurrentBid, opt => opt.MapFrom(src => src.MinimumBid))
            .ForMember(dest => dest.BidStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<Item, ItemDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom<ItemToItemDtoResolver>())
            .ForMember(dest => dest.Documents, opt => opt.MapFrom<ItemToItemDtoDocumentResolver>())
            .ForMember(dest => dest.SellerUsername, opt => opt.MapFrom(src => src.Seller.Username));
        CreateMap<Bid, BidDto>()
            .ForMember(dest => dest.BidderUsername, opt => opt.MapFrom(src => src.Bidder.Username));
    }
}

public class ItemToItemDtoResolver : IValueResolver<Item, ItemDto, List<string>>
{
    private readonly IConfiguration _configuration;

    public ItemToItemDtoResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<string> Resolve(Item source, ItemDto destination, List<string> destMember, ResolutionContext context)
    {
        return source.Images.Select(image => _configuration["Url"] + image.ImageUrl).ToList();
    }
}

public class ItemToItemDtoDocumentResolver : IValueResolver<Item, ItemDto, List<string>>
{
    private readonly IConfiguration _configuration;

    public ItemToItemDtoDocumentResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<string> Resolve(Item source, ItemDto destination, List<string> destMember, ResolutionContext context)
    {
        return source.Documents.Select(doc => _configuration["Url"] + doc.DocumentUrl).ToList();
    }
}