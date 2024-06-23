using AutoMapper;
using BricksHoarder.Core.Commands;

namespace BricksHoarder.Common.CQRS
{
    public class RequestToCommandMapper
    {
        private readonly IMapper _mapper;

        public RequestToCommandMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TCommand Map<TRequest, TCommand>(TRequest request)
            where TCommand : class, ICommand
            where TRequest : class, IRequest
        {
            return _mapper.Map<TRequest, TCommand>(request);
        }

        public TCommand Map<TRequest, TCommand>(TRequest request, Action<TCommand> afterMap)
            where TCommand : class, ICommand
            where TRequest : class, IRequest
        {
            var command = _mapper.Map<TRequest, TCommand>(request);
            afterMap(command);
            return command;
        }
    }
}