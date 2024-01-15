using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Services;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = FluentValidation.ValidationException;

namespace BricksHoarder.AzureCloud.ServiceBus
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
            IValidator<TCommand>? validator = _context.GetService<IValidator<TCommand>>();
            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(command);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            Guid correlationId = _guidService.New;
            var commandName = command.GetType().Name;

            ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{commandName}"));
            await endpoint.Send(command, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }

        public async Task<Guid> DispatchAsync<TRequest, TCommand>(TRequest request)
            where TCommand : class, ICommand
            where TRequest : class, IRequest
        {
            TCommand command = _requestToCommandMapper.Map<TRequest, TCommand>(request);
            return await DispatchAsync(command);
        }

        public async Task<Guid> DispatchAsync<TRequest, TCommand>(TRequest request, Action<TCommand> afterMap)
            where TCommand : class, ICommand
            where TRequest : class, IRequest
        {
            TCommand command = _requestToCommandMapper.Map(request, afterMap);
            return await DispatchAsync(command);
        }
    }
}