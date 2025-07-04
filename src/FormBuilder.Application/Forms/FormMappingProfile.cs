using AutoMapper;
using FormBuilder.Application.Contract.Forms.Dtos.Response;
using FormBuilder.Domain.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Forms;

public class FormMappingProfile : Profile
{
    public FormMappingProfile()
    {
        CreateMap<Form, FormResponse>().ReverseMap();
    }
}
