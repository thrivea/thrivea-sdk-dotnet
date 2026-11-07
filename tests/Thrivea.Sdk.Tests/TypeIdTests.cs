using Thrivea.Ids;

namespace Thrivea.Sdk.Tests;

public class TypeIdTests
{
    [Theory]
    [MemberData(nameof(TypeIdSpecVectors.Valid), MemberType = typeof(TypeIdSpecVectors))]
    public void Spec_vector_decodes_to_its_uuid_and_encodes_back(string name, string typeid, string prefix, string uuid)
    {
        if (prefix.Length == 0)
        {
            // The SDK only deals in prefixed ids; a bare suffix is checked through the codec by prefixing it.
            typeid = "x_" + typeid;
            prefix = "x";
        }

        Assert.True(TypeId.TryParse(typeid, out var parsed), $"{name}: '{typeid}' should parse");
        Assert.Equal(prefix, parsed.Prefix);
        Assert.Equal(Guid.Parse(uuid), parsed.Uuid);
        Assert.Equal(typeid, TypeId.FromUuid(prefix, Guid.Parse(uuid)).ToString());
    }

    [Theory]
    [MemberData(nameof(TypeIdSpecVectors.Invalid), MemberType = typeof(TypeIdSpecVectors))]
    public void Spec_invalid_vector_is_refused(string name, string typeid, string description)
    {
        Assert.False(TypeId.TryParse(typeid, out _), $"{name}: '{typeid}' should be refused ({description})");
    }

    [Fact]
    public void Round_trips_random_uuids()
    {
        for (var i = 0; i < 1000; i++)
        {
            var uuid = Guid.NewGuid();
            var id = TypeId.FromUuid(ResourcePrefixes.JobOpening, uuid);
            Assert.StartsWith("job_", id.ToString(), StringComparison.Ordinal);
            Assert.Equal("job_".Length + 26, id.ToString().Length);
            Assert.Equal(uuid, TypeId.Parse(id.ToString()).Uuid);
        }
    }

    [Fact]
    public void Prefix_mismatch_is_a_clear_argument_error()
    {
        var candidate = TypeId.FromUuid(ResourcePrefixes.Candidate, Guid.NewGuid()).ToString();

        var error = Assert.Throws<ArgumentException>(() => TypeId.EnsurePrefix(candidate, ResourcePrefixes.JobOpening, "jobOpeningId"));

        Assert.Contains("'cand' id", error.Message, StringComparison.Ordinal);
        Assert.Contains("'job_…'", error.Message, StringComparison.Ordinal);
        Assert.Equal("jobOpeningId", error.ParamName);
    }

    [Fact]
    public void Parse_with_expected_prefix_checks_it()
    {
        var id = TypeId.FromUuid("emp", Guid.NewGuid()).ToString();

        Assert.Equal("emp", TypeId.Parse(id, "emp").Prefix);
        Assert.Throws<FormatException>(() => TypeId.Parse(id, "job"));
        Assert.Throws<FormatException>(() => TypeId.Parse("not an id"));
    }

    [Fact]
    public void Default_value_is_harmless()
    {
        var id = default(TypeId);

        Assert.Equal(string.Empty, id.Prefix);
        Assert.Equal("_", id.ToString());
        Assert.Equal(id, default);
    }

    [Fact]
    public void Equality_is_by_prefix_and_uuid()
    {
        var uuid = Guid.NewGuid();

        Assert.Equal(TypeId.FromUuid("job", uuid), TypeId.Parse(TypeId.FromUuid("job", uuid).ToString()));
        Assert.NotEqual(TypeId.FromUuid("job", uuid), TypeId.FromUuid("cand", uuid));
        Assert.True(TypeId.IsValid(TypeId.FromUuid("job", uuid).ToString(), "job"));
        Assert.False(TypeId.IsValid(TypeId.FromUuid("job", uuid).ToString(), "cand"));
    }
}
