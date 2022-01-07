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

    public static T FirstNotNullOrDefault<T>(this IEnumerable<T> source)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));

        foreach (var item in source)
        {
            if (item != null)
            {
                return item;
            }
        }

        return default;
    }
}
