using AutoMapper;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Events;

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