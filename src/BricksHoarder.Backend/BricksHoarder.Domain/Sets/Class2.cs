using AutoMapper;

namespace BricksHoarder.Domain.Sets
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateSetCommand, SetCreated>();
        }
    }
}