using System.Runtime.CompilerServices;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary>What every resource service shares; nothing here is for callers.</summary>
public abstract class ThriveaService
{
    internal ThriveaService(ThriveaHttpTransport transport)
    {
        Transport = transport;
    }

    internal ThriveaHttpTransport Transport { get; }

    /// <summary>Walks every page: fetches with <paramref name="fetch" />, then again from the last id, until <c>has_more</c> is false.</summary>
    internal static async IAsyncEnumerable<T> AutoPageAsync<T, TOptions>(
        TOptions? options,
        Func<TOptions, CancellationToken, Task<ThriveaList<T>>> fetch,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : IThriveaResource
        where TOptions : ListOptions, new()
    {
        var page = (TOptions)(options ?? new TOptions()).CloneForPaging();
        page.EndingBefore = null;
        page.Limit ??= 100;

        while (true)
        {
            var list = await fetch(page, cancellationToken).ConfigureAwait(false);
            foreach (var item in list.Data)
            {
                yield return item;
            }

            if (!list.HasMore || list.Data.Count == 0)
            {
                yield break;
            }

            page.StartingAfter = list.Data[list.Data.Count - 1].Id;
        }
    }
}
