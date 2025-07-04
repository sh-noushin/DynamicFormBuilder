using AutoMapper;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Response;
using FormBuilder.Domain.FormControlValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormControlValues;

public class FormControlValueMappoinProfile : Profile
{
    public FormControlValueMappoinProfile()
    {
        CreateMap<FormControlValue, FormControlValueResponse>().ReverseMap();
    }
}
