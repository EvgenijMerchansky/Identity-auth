using AutoMapper;
using Identity_auth.Models;

namespace Identity_auth.Configurations.MapperProfiles
{
    public class AccountControllerProfile : Profile
    {
        public AccountControllerProfile()
        {
            CreateMap<LoginModel, RegisterModel>()
                .ReverseMap();

            CreateMap<FacebookLoginModel, RegisterModel>();
        }
    }
}