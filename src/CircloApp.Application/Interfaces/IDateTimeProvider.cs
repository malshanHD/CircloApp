namespace CircloApp.Application.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
