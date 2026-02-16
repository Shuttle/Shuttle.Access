using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class GeneratePasswordParticipant(IPasswordGenerator passwordGenerator) : IParticipant<GeneratePassword>
{
    private readonly IPasswordGenerator _passwordGenerator = Guard.AgainstNull(passwordGenerator);

    public async Task HandleAsync(GeneratePassword message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        message.GeneratedPassword = _passwordGenerator.Generate();

        await Task.CompletedTask;
    }
}