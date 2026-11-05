namespace Transactions.IntegrationTests.Support;

// Polls until a condition holds, for work that happens in the background, such as handling a message.
public static class Eventually
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

    // Returns the last value read, whether or not the condition was met, so the test's asserts show what went wrong.
    public static async Task<T> GetAsync<T>(Func<Task<T>> read, Func<T, bool> until, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);

        while (true)
        {
            var value = await read();

            if (until(value) || DateTime.UtcNow >= deadline)
            {
                return value;
            }

            await Task.Delay(PollInterval, TestContext.Current.CancellationToken);
        }
    }
}