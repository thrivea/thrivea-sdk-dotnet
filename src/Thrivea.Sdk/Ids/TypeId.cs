using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Thrivea.Ids;

/// <summary>
///     A Thrivea identifier: a resource prefix, an underscore and 26 characters that encode a UUID, for example
///     <c>job_01h2xcejqtf2nbrexx3vqjhp41</c>. Ids are sortable by creation time and safe in URLs. Store them as the strings
///     the API gives you; parse one only when you need its prefix or its UUID.
///     <para>Format: <see href="https://github.com/jetify-com/typeid">TypeID</see> (Crockford base32, lowercase).</para>
/// </summary>
public readonly struct TypeId : IEquatable<TypeId>
{
    private const string Alphabet = "0123456789abcdefghjkmnpqrstvwxyz";
    private const int SuffixLength = 26;
    private const char Separator = '_';

    private static readonly sbyte[] Lookup = BuildLookup();

    private readonly string? _prefix;
    private readonly string? _suffix;

    private TypeId(string prefix, string suffix, Guid uuid)
    {
        _prefix = prefix;
        _suffix = suffix;
        Uuid = uuid;
    }

    /// <summary>The resource prefix, such as <c>job</c>; see <see cref="ResourcePrefixes" />.</summary>
    public string Prefix => _prefix ?? string.Empty;

    /// <summary>The 26-character encoded part after the underscore.</summary>
    public string Suffix => _suffix ?? string.Empty;

    /// <summary>The UUID the suffix encodes.</summary>
    public Guid Uuid { get; }

    /// <summary>Makes the id of a <paramref name="prefix" /> resource from its UUID.</summary>
    public static TypeId FromUuid(string prefix, Guid uuid)
    {
        ValidatePrefix(prefix);
        return new TypeId(prefix, EncodeSuffix(uuid), uuid);
    }

    /// <summary>Parses <paramref name="value" />, or throws <see cref="FormatException" />.</summary>
    public static TypeId Parse(string value)
    {
        return TryParse(value, out var id) ? id : throw new FormatException($"'{value}' is not a Thrivea id (prefix_ followed by 26 characters).");
    }

    /// <summary>Parses <paramref name="value" /> and checks that it belongs to <paramref name="expectedPrefix" />.</summary>
    public static TypeId Parse(string value, string expectedPrefix)
    {
        var id = Parse(value);
        if (!id.HasPrefix(expectedPrefix))
        {
            throw new FormatException($"'{value}' is a '{id.Prefix}' id; expected a '{expectedPrefix}_…' id.");
        }

        return id;
    }

    /// <summary>Parses <paramref name="value" /> without throwing.</summary>
    public static bool TryParse([NotNullWhen(true)] string? value, out TypeId id)
    {
        id = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var separator = value!.LastIndexOf(Separator);
        if (separator <= 0 || separator != value.Length - SuffixLength - 1)
        {
            return false;
        }

        var prefix = value.Substring(0, separator);
        if (!IsValidPrefix(prefix))
        {
            return false;
        }

        var suffix = value.Substring(separator + 1);
        if (!TryDecodeSuffix(suffix, out var uuid))
        {
            return false;
        }

        id = new TypeId(prefix, suffix, uuid);
        return true;
    }

    /// <summary>Whether <paramref name="value" /> is a well-formed id of the <paramref name="prefix" /> resource.</summary>
    public static bool IsValid([NotNullWhen(true)] string? value, string prefix)
    {
        return TryParse(value, out var id) && id.HasPrefix(prefix);
    }

    /// <summary>Whether this id belongs to the <paramref name="prefix" /> resource.</summary>
    public bool HasPrefix(string prefix)
    {
        return string.Equals(Prefix, prefix, StringComparison.Ordinal);
    }

    /// <summary>The id as the API writes it: <c>prefix_suffix</c>.</summary>
    public override string ToString()
    {
        return Prefix + Separator + Suffix;
    }

    /// <inheritdoc />
    public bool Equals(TypeId other)
    {
        return string.Equals(Prefix, other.Prefix, StringComparison.Ordinal) && Uuid.Equals(other.Uuid);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TypeId other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (StringComparer.Ordinal.GetHashCode(Prefix) * 397) ^ Uuid.GetHashCode();
        }
    }

    /// <summary>Value equality.</summary>
    public static bool operator ==(TypeId left, TypeId right)
    {
        return left.Equals(right);
    }

    /// <summary>Value inequality.</summary>
    public static bool operator !=(TypeId left, TypeId right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Throws <see cref="ArgumentException" /> when <paramref name="value" /> is not an id of the
    ///     <paramref name="prefix" /> resource. The services call it before a request leaves, so a wrong id fails at once
    ///     with a message naming the parameter rather than as a <c>400</c> from the API.
    /// </summary>
    internal static void EnsurePrefix(string? value, string prefix, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required: a '{prefix}_…' id.", parameterName);
        }

        if (!TryParse(value, out var id))
        {
            throw new ArgumentException($"{parameterName} is not a Thrivea id: '{value}'. Expected '{prefix}_' followed by 26 characters.", parameterName);
        }

        if (!id.HasPrefix(prefix))
        {
            throw new ArgumentException($"{parameterName} is a '{id.Prefix}' id ('{value}'); expected a '{prefix}_…' id.", parameterName);
        }
    }

    private static void ValidatePrefix(string prefix)
    {
        if (!IsValidPrefix(prefix))
        {
            throw new ArgumentException("A prefix is 1 to 63 lowercase ASCII letters, with single underscores allowed inside.", nameof(prefix));
        }
    }

    private static bool IsValidPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || prefix.Length > 63 || prefix[0] == Separator || prefix[prefix.Length - 1] == Separator)
        {
            return false;
        }

        foreach (var c in prefix)
        {
            if ((c < 'a' || c > 'z') && c != Separator)
            {
                return false;
            }
        }

        return true;
    }

    // The UUID's 16 bytes in RFC 4122 order (which Guid.ToString("N") gives on every runtime), as 26 base32 characters
    // read from the least significant end. 26 × 5 = 130 bits, so the first character carries only the top 2 bits.
    private static string EncodeSuffix(Guid uuid)
    {
        var bytes = HexToBytes(uuid.ToString("N"));
        ulong high = 0, low = 0;
        for (var i = 0; i < 8; i++)
        {
            high = (high << 8) | bytes[i];
            low = (low << 8) | bytes[i + 8];
        }

        var chars = new char[SuffixLength];
        for (var i = SuffixLength - 1; i >= 0; i--)
        {
            chars[i] = Alphabet[(int)(low & 0x1F)];
            low = (low >> 5) | (high << 59);
            high >>= 5;
        }

        return new string(chars);
    }

    private static bool TryDecodeSuffix(string suffix, out Guid uuid)
    {
        uuid = Guid.Empty;
        if (suffix.Length != SuffixLength)
        {
            return false;
        }

        ulong high = 0, low = 0;
        for (var i = 0; i < SuffixLength; i++)
        {
            var c = suffix[i];
            var value = c < 128 ? Lookup[c] : -1;
            if (value < 0 || (i == 0 && value > 7))
            {
                return false;
            }

            high = (high << 5) | (low >> 59);
            low = (low << 5) | (uint)value;
        }

        var bytes = new byte[16];
        for (var i = 7; i >= 0; i--)
        {
            bytes[i] = (byte)(high & 0xFF);
            bytes[i + 8] = (byte)(low & 0xFF);
            high >>= 8;
            low >>= 8;
        }

        uuid = new Guid(BytesToHex(bytes));
        return true;
    }

    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    private static string BytesToHex(byte[] bytes)
    {
        var chars = new char[bytes.Length * 2];
        const string digits = "0123456789abcdef";
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = digits[bytes[i] >> 4];
            chars[i * 2 + 1] = digits[bytes[i] & 0xF];
        }

        return new string(chars);
    }

    private static sbyte[] BuildLookup()
    {
        var lookup = new sbyte[128];
        for (var i = 0; i < lookup.Length; i++)
        {
            lookup[i] = -1;
        }

        for (var i = 0; i < Alphabet.Length; i++)
        {
            lookup[Alphabet[i]] = (sbyte)i;
        }

        return lookup;
    }
}
