namespace CircloApp.Infrastructure.Cache
{
    public class RedisSettings
    {
        public const string SectionName = "RedisSettings";
        public string ConnectionString { get; set; } = string.Empty;
        public int DefaultExpiryMinutes { get; set; }
    }
}
