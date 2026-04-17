using NUnit.Framework;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

[SetUpFixture]
public class ApiFixture
{
    [OneTimeSetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("CONFIGURATION_FOLDER", ".");
    }
}