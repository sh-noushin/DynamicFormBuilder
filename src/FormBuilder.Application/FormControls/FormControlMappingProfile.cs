using AutoMapper;
using FormBuilder.Application.Contract.FormControls.Dtos.Response;
using FormBuilder.Domain.FormControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormControls;

public class FormControlMappingProfile : Profile
{
    public FormControlMappingProfile()
    {
        CreateMap<FormControl, FormControlResponse>().ReverseMap();
    }
}
