namespace Shuttle.Access
{
    public class AccessConfiguration : IAccessConfiguration
    {
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
    }
}