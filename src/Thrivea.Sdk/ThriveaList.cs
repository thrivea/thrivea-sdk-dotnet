using System.Text.Json.Serialization;

namespace Thrivea;

/// <summary>Something the API identifies: every resource has an <see cref="Id" /> and says its <see cref="Object" /> type.</summary>
public interface IThriveaResource
{
    /// <summary>The id, such as <c>job_01h2xcejqtf2nbrexx3vqjhp41</c>.</summary>
    string Id { get; }

    /// <summary>The resource's type name, such as <c>job_opening</c>.</summary>
    string Object { get; }
}

/// <summary>
///     One page of a list: the items and whether more follow. <c>foreach</c> over it, or page on with
///     <see cref="ListOptions.StartingAfter" />. (It deliberately does not implement <see cref="IEnumerable{T}" />:
///     System.Text.Json would then read it as a JSON array rather than the <c>{ data, has_more }</c> envelope it is.)
/// </summary>
public sealed class ThriveaList<T>
{
    /// <summary>Always <c>list</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "list";

    /// <summary>The list's path, such as <c>/v1/job_openings</c>.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>The items on this page.</summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();

    /// <summary>Whether another page follows this one.</summary>
    [JsonPropertyName("has_more")]
    public bool HasMore { get; init; }

    /// <summary>Enumerates <see cref="Data" />, so <c>foreach (var item in page)</c> works.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        return Data.GetEnumerator();
    }
}

/// <summary>The paging parameters every list takes.</summary>
public class ListOptions
{
    /// <summary>How many items to return, 1 to 100. The API's default is 10.</summary>
    public int? Limit { get; set; }

    /// <summary>Return the items after this id: the last id of the page you have.</summary>
    public string? StartingAfter { get; set; }

    /// <summary>Return the items before this id, for paging backwards where the resource supports it.</summary>
    public string? EndingBefore { get; set; }

    internal virtual void WriteTo(Internal.QueryStringBuilder query)
    {
        query.Add("limit", Limit).Add("starting_after", StartingAfter).Add("ending_before", EndingBefore);
    }

    internal Internal.QueryStringBuilder ToQuery()
    {
        var query = new Internal.QueryStringBuilder();
        WriteTo(query);
        return query;
    }

    internal ListOptions CloneForPaging()
    {
        return (ListOptions)MemberwiseClone();
    }
}

/// <summary>What a <c>DELETE</c> returns: the id of what is gone.</summary>
public sealed class DeletedResource : IThriveaResource
{
    /// <inheritdoc />
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("object")]
    public string Object { get; init; } = string.Empty;

    /// <summary>Always <c>true</c>.</summary>
    [JsonPropertyName("deleted")]
    public bool Deleted { get; init; }
}
