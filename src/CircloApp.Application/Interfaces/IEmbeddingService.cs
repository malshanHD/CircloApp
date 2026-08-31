namespace CircloApp.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<ReadOnlyMemory<float>> GenerateEmbeddingAync(string text, CancellationToken cancellationToken = default);
    }
}
