using AutoMapper;
using FormBuilder.Core.DTOs;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity -> read DTO (one-way; prevents clients from writing server-owned
        // fields like CreatedAt/UpdatedAt via a round-trip mapping).
        CreateMap<Form, FormDto>();
        CreateMap<FormVersion, FormVersionDto>();
        CreateMap<FormVersionField, FormFieldDto>();
        CreateMap<FormSubmission, FormSubmissionDto>();
        CreateMap<FormSubmissionValue, FormSubmissionValueDto>();

        // Create/update DTO -> entity (one-way; services materialize entities
        // from client-provided DTOs when persisting).
        CreateMap<CreateFormDto, Form>();
        CreateMap<UpdateFormDto, Form>();
        CreateMap<CreateFormVersionDto, FormVersion>();
        CreateMap<UpdateFormVersionDto, FormVersion>();
        CreateMap<CreateFormFieldDto, FormVersionField>();
        CreateMap<UpdateFormFieldDto, FormVersionField>();
        CreateMap<UpdateFormSubmissionDto, FormSubmission>();

        CreateMap<CreateFormSubmissionDto, FormSubmission>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src =>
                (src.FieldValues ?? new Dictionary<string, string?>())
                    .Select(kv => new FormSubmissionValue { FieldName = kv.Key, FieldValue = kv.Value })
                    .ToList()));

        // User -> UserDto is the only mapping the services actually use for the
        // User entity; role and CreatedAt are populated by the service itself.
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
}
