using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Services;
using MassTransit;

namespace BricksHoarder.Kafka
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _context;
        private readonly IGuidService _guidService;
        private readonly RequestToCommandMapper _requestToCommandMapper;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public CommandDispatcher(
            IServiceProvider context,
            ISendEndpointProvider sendEndpointProvider,
            IGuidService guidService,
            RequestToCommandMapper requestToCommandMapper)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
            _guidService = guidService;
            _requestToCommandMapper = requestToCommandMapper;
        }

        public async Task<Guid> DispatchAsync<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> DispatchAsync<TRequest, TCommand>(TRequest request) where TRequest : class, IRequest where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> DispatchAsync<TRequest, TCommand>(TRequest request, Action<TCommand> afterMap) where TRequest : class, IRequest where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> ScheduleDispatchAsync<TCommand>(TCommand command, DateTime when) where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> ScheduleDispatchAsync<TCommand>(TCommand command, TimeSpan @from) where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }
    }
}