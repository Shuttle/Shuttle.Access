using System.Collections.Generic;

namespace Shuttle.Access.Messages;

public class Identifiers<T>
{
    public List<T> Values { get; set; } = [];
}