using AutoMapper;
using ArogyaPulse.Api.Models;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientResponseDto>()
                .ForMember(dest => dest.Vitals, opt => opt.MapFrom(src => new VitalsDto
                {
                    Bp = src.Bp,
                    SpO2 = src.SpO2,
                    Temp = src.Temp,
                    Glucose = src.Glucose
                }));

            CreateMap<PatientCreateDto, Patient>()
                .ForMember(dest => dest.Bp, opt => opt.MapFrom(src => src.Vitals.Bp))
                .ForMember(dest => dest.SpO2, opt => opt.MapFrom(src => src.Vitals.SpO2))
                .ForMember(dest => dest.Temp, opt => opt.MapFrom(src => src.Vitals.Temp))
                .ForMember(dest => dest.Glucose, opt => opt.MapFrom(src => src.Vitals.Glucose))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RiskScore, opt => opt.Ignore())
                .ForMember(dest => dest.RiskLevel, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.DoctorNotes, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore());
        }
    }
}