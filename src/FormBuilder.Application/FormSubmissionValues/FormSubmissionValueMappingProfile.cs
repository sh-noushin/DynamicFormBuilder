using AutoMapper;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;
using FormBuilder.Domain.FormSubmissionValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormSubmissionValues;

public class FormSubmissionValueMappingProfile : Profile
{
    public FormSubmissionValueMappingProfile()
    {
        CreateMap<FormSubmissionValue, FormSubmissionValueResponse>().ReverseMap();
    }
}
