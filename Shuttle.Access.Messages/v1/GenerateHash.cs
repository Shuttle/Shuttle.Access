using System;

namespace Shuttle.Access.Messages.v1;

public class GenerateHash
{
    public byte[] Hash { get; set; } = [];
    public string Value { get; set; } = string.Empty;
}