using AutoMapper;
using FormBuilder.Models.Entities;
using FormBuilder.Core.DTOs;

namespace FormBuilder.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Form, FormDto>().ReverseMap();
            CreateMap<Form, CreateFormDto>().ReverseMap();
            CreateMap<Form, UpdateFormDto>().ReverseMap();

            CreateMap<FormVersion, FormVersionDto>().ReverseMap();
            CreateMap<FormVersion, CreateFormVersionDto>().ReverseMap();
            CreateMap<FormVersion, UpdateFormVersionDto>().ReverseMap();

            CreateMap<FormVersionField, FormFieldDto>().ReverseMap();
            CreateMap<FormVersionField, CreateFormFieldDto>().ReverseMap();
            CreateMap<FormVersionField, UpdateFormFieldDto>().ReverseMap();

            CreateMap<FormSubmission, FormSubmissionDto>().ReverseMap();
            CreateMap<FormSubmission, CreateFormSubmissionDto>().ReverseMap();
            CreateMap<FormSubmission, UpdateFormSubmissionDto>().ReverseMap();

            CreateMap<FormSubmissionValue, FormSubmissionValueDto>().ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); 

            CreateMap<User, RegisterUserDto>().ReverseMap();
            CreateMap<User, UpdateUserDto>().ReverseMap();


            CreateMap<UserRole, UserRole>().ReverseMap();
            CreateMap<FieldType, FieldType>().ReverseMap();
        }
    }
}
