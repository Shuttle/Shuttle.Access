using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class GeneratePasswordParticipant : IParticipant<GeneratePassword>
{
    private readonly IPasswordGenerator _passwordGenerator;

    public GeneratePasswordParticipant(IPasswordGenerator passwordGenerator)
    {
        _passwordGenerator = Guard.AgainstNull(passwordGenerator);
    }

    public async Task ProcessMessageAsync(IParticipantContext<GeneratePassword> context)
    {
        Guard.AgainstNull(context);

        context.Message.GeneratedPassword = _passwordGenerator.Generate();

        await Task.CompletedTask;
    }
}