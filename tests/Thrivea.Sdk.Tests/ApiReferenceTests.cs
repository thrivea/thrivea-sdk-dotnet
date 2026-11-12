using System.Text.Json;

namespace Thrivea.Sdk.Tests;

/// <summary>
///     <c>docs/API-reference.md</c> is generated from the vendored OpenAPI document, so it lists every operation and every
///     field there is. When the document is vendored again the page must be regenerated in the same change: run the tests
///     with <c>THRIVEA_UPDATE_REFERENCE=1</c> and commit the file.
/// </summary>
public class ApiReferenceTests
{
    [Fact]
    public void The_reference_page_is_what_the_vendored_document_renders()
    {
        var root = RepositoryRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "openapi", "thrivea-v1.openapi.json")));
        var rendered = ApiReference.Render(document.RootElement);
        var path = Path.Combine(root, "docs", "API-reference.md");

        if (Environment.GetEnvironmentVariable("THRIVEA_UPDATE_REFERENCE") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, rendered);
        }

        Assert.True(File.Exists(path), "docs/API-reference.md is missing; run the tests with THRIVEA_UPDATE_REFERENCE=1.");
        Assert.True(
            File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal) == rendered,
            "docs/API-reference.md is out of date with openapi/thrivea-v1.openapi.json; run the tests with THRIVEA_UPDATE_REFERENCE=1 and commit it.");
    }

    [Fact]
    public void Every_operation_and_every_type_is_on_the_page()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryRoot(), "openapi", "thrivea-v1.openapi.json")));
        var rendered = ApiReference.Render(document.RootElement);

        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                Assert.Contains($"### {method.Name.ToUpperInvariant()} {path.Name}", rendered, StringComparison.Ordinal);
            }
        }

        foreach (var schema in document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject())
        {
            Assert.Contains($"### {schema.Name}\n", rendered, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("GET /v1/job_openings/{id}", "get-v1job_openingsid")]
    [InlineData("CandidateApplication", "candidateapplication")]
    public void Anchors_are_the_ones_GitHub_makes(string heading, string anchor)
    {
        Assert.Equal(anchor, ApiReference.Anchor(heading));
    }

    [Fact]
    public void An_id_reads_as_its_prefix_not_as_a_uuid()
    {
        using var schema = JsonDocument.Parse("""{"type":"string","pattern":"^job_[0-7][0-9a-hjkmnp-tv-z]{25}$"}""");

        Assert.Equal("id (`job_…`)", ApiReference.TypeOf(schema.RootElement));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Thrivea.Sdk.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("The repository root (Thrivea.Sdk.slnx) was not found.");
    }
}
