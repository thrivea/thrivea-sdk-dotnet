using System.Globalization;
using System.Text;

namespace Thrivea.Internal;

/// <summary>Builds the query string of a list request; absent values are left out.</summary>
internal sealed class QueryStringBuilder
{
    private readonly List<KeyValuePair<string, string>> _parameters = new();

    public QueryStringBuilder Add(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value!));
        }

        return this;
    }

    public QueryStringBuilder Add(string key, int? value)
    {
        return value.HasValue ? Add(key, value.Value.ToString(CultureInfo.InvariantCulture)) : this;
    }

    public QueryStringBuilder Add(string key, DateTimeOffset? value)
    {
        // As an instant in UTC with a trailing Z, the one spelling of a time the API never misreads.
        return value.HasValue ? Add(key, value.Value.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)) : this;
    }

    public int Count => _parameters.Count;

    public override string ToString()
    {
        if (_parameters.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder("?");
        for (var i = 0; i < _parameters.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(_parameters[i].Key)).Append('=').Append(Uri.EscapeDataString(_parameters[i].Value));
        }

        return builder.ToString();
    }
}
