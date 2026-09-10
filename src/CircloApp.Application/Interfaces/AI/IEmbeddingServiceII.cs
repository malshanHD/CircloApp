namespace CircloApp.Application.Interfaces.AI
{
    public interface IEmbeddingServiceII
    {
        Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
    }
}
