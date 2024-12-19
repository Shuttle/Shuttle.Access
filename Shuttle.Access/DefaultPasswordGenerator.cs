using System;

namespace Shuttle.Access;

public class DefaultPasswordGenerator : IPasswordGenerator
{
    private const string Lowercase = "bcdfghjkmnpqrstvwxyz";
    private const string Uppercase = "BCDFGHJKMNPQRSTVWXYZ";
    private const string Digits = "23456789";
    private const string Special = "!@#$%^&*()[]";

    private static readonly Random Random = new();

    public string Generate()
    {
        return Pick(Uppercase) +
               Pick(Lowercase) +
               Pick(Lowercase) +
               Pick(Special) +
               Pick(Uppercase) +
               Pick(Lowercase) +
               Pick(Lowercase) +
               Pick(Digits);
    }

    private static string Pick(string characters)
    {
        return characters[Random.Next(0, characters.Length - 1)].ToString();
    }
}