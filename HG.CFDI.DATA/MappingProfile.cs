using AutoMapper;
using CFDI.Data.Entities;

//using HG.CFDI.API.Models;
//using HG.CFDI.CORE.Models.CartaPorteEF;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.DATA
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<cartaPorteCabecera, cartaPorteCabecera>()
                .ForMember(dest => dest.id, opt => opt.Ignore()) // Ignorar el id
                .ForMember(dest => dest.cartaPorteDetalles, opt => opt.MapFrom(src => src.cartaPorteDetalles))
                .ForMember(dest => dest.cartaPorteAddenda, opt => opt.MapFrom(src => src.cartaPorteAddenda))
                .ForMember(dest => dest.cartaPorteMercancia, opt => opt.MapFrom(src => src.cartaPorteMercancia))
                .ForMember(dest => dest.cartaPorteUbicaciones, opt => opt.MapFrom(src => src.cartaPorteUbicaciones))
                .ForMember(dest => dest.cartaPorteRegimenAduaneros, opt => opt.MapFrom(src => src.cartaPorteRegimenAduaneros))
                 .ForMember(dest => dest.cartaPorteSustitucions, opt => opt.MapFrom(src => src.cartaPorteSustitucions))
                .ForMember(dest => dest.archivoCFDi, opt => opt.MapFrom(src => src.archivoCFDi));

            CreateMap<cartaPorteDetalle, cartaPorteDetalle>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<cartaPorteAddendum, cartaPorteAddendum>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<cartaPorteMercancium, cartaPorteMercancium>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<cartaPorteUbicacione, cartaPorteUbicacione>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<cartaPorteRegimenAduanero, cartaPorteRegimenAduanero>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<cartaPorteSustitucion, cartaPorteSustitucion>().ForMember(dest => dest.id, opt => opt.Ignore());
            CreateMap<archivoCFDi, archivoCFDi>().ForMember(dest => dest.idArchivoCFDi, opt => opt.Ignore());
        }
    }

}




