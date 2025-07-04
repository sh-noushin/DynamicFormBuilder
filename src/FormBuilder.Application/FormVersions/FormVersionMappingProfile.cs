using AutoMapper;
using FormBuilder.Application.Contract.FormVersions.Dtos.Response;
using FormBuilder.Domain.FormVersions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormVersions;

public class FormVersionMappingProfile : Profile
{
    public FormVersionMappingProfile()
    {
        CreateMap<FormVersion, FormVersionResponse>().ReverseMap();
    }
}
