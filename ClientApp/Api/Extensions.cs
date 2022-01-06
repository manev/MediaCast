namespace MediaCast.Api;

internal static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));

        action = action ?? throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
        {
            action(item);
        }
    }
}
