using AutoMapper;
using FormBuilder.Application.Contract.FormSubmissions.Dtos.Response;
using FormBuilder.Domain.FormSubmissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormSubmissions;

public class FormSubmissionMappingProfile : Profile
{
    public FormSubmissionMappingProfile()
    {
        CreateMap<FormSubmission, FormSubmissionResponse>().ReverseMap();
    }
}
