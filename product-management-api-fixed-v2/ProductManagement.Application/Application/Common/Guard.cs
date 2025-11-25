public static class Guard
{
    public static T NotNull<T>(T? value, string name) where T : class
    {
        if (value is null)
            throw new InvalidOperationException($"{name} must not be null.");
        return value;
    }

    public static IReadOnlyCollection<T> NotNullCollection<T>(IReadOnlyCollection<T>? value, string name)
    {
        if (value is null)
            throw new InvalidOperationException($"{name} must not be null.");
        return value;
    }
}
