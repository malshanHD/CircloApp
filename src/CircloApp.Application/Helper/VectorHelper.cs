namespace CircloApp.Application.Helper
{
    public static class VectorHelper
    {
        public static double CosineSimilarity(
        ReadOnlyMemory<float> vectorA,
        ReadOnlyMemory<float> vectorB)
        {
            var a = vectorA.Span;
            var b = vectorB.Span;

            if (a.Length != b.Length)
            {
                throw new ArgumentException(
                    "Vectors must have the same dimensions.");
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (var i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];

                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            return dotProduct /
                   (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}
