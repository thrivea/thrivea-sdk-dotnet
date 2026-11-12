using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Thrivea.Sdk.Tests;

/// <summary>
///     Renders the vendored OpenAPI document as one Markdown page: every operation with its scope, parameters, body and
///     answers, then every type with every field. It is generated, never written by hand, so the reference cannot drift from
///     the contract the SDK is tested against. <see cref="ApiReferenceTests" /> keeps <c>docs/API-reference.md</c> current.
/// </summary>
internal static class ApiReference
{
    private static readonly string[] Methods = ["get", "post", "put", "patch", "delete"];

    public static string Render(JsonElement root)
    {
        var markdown = new StringBuilder();
        var schemas = root.GetProperty("components").GetProperty("schemas");

        markdown.AppendLine("# API reference");
        markdown.AppendLine();
        markdown.AppendLine("Every operation and every type of the Thrivea public API (`/v1`), generated from the OpenAPI document in");
        markdown.AppendLine("[`openapi/thrivea-v1.openapi.json`](https://github.com/thrivea/thrivea-sdk-dotnet/blob/main/openapi/thrivea-v1.openapi.json).");
        markdown.AppendLine("Do not edit it by hand: it is regenerated whenever the document changes. How to use each resource, with examples, is");
        markdown.AppendLine("on its own page; this one is the complete list.");
        markdown.AppendLine();
        markdown.AppendLine("Base URL `https://api.thrivea.com`. Every call sends `Authorization: Bearer <API key>`, and the key needs the scope the");
        markdown.AppendLine("operation names. Every answer carries `Request-Id` and the `RateLimit-*` headers; every write accepts `Idempotency-Key`.");
        markdown.AppendLine("Ids are TypeIDs: a prefix, an underscore and 26 characters, such as `job_01k9m3z4r7f8n9pq2s3t4v5w6x`.");
        markdown.AppendLine();

        var operations = Operations(root.GetProperty("paths")).ToList();

        markdown.AppendLine("## Operations");
        markdown.AppendLine();
        markdown.AppendLine("| Operation | What it does | Scope |");
        markdown.AppendLine("|---|---|---|");
        foreach (var operation in operations)
        {
            markdown.AppendLine($"| [`{operation.Method} {operation.Path}`](#{Anchor($"{operation.Method} {operation.Path}")}) | {Cell(Text(operation.Body, "summary"))} | {Cell(Scopes(operation.Body))} |");
        }

        markdown.AppendLine();
        foreach (var operation in operations)
        {
            RenderOperation(markdown, operation);
        }

        markdown.AppendLine("## Types");
        markdown.AppendLine();
        foreach (var schema in schemas.EnumerateObject().OrderBy(schema => schema.Name, StringComparer.Ordinal))
        {
            RenderType(markdown, schema.Name, schema.Value);
        }

        return markdown.ToString().TrimEnd() + "\n";
    }

    private static IEnumerable<(string Method, string Path, JsonElement Body)> Operations(JsonElement paths)
    {
        foreach (var path in paths.EnumerateObject().OrderBy(path => path.Name, StringComparer.Ordinal))
        {
            foreach (var method in Methods)
            {
                if (path.Value.TryGetProperty(method, out var body))
                {
                    yield return (method.ToUpperInvariant(), path.Name, body);
                }
            }
        }
    }

    private static void RenderOperation(StringBuilder markdown, (string Method, string Path, JsonElement Body) operation)
    {
        markdown.AppendLine($"### {operation.Method} {operation.Path}");
        markdown.AppendLine();
        AppendParagraph(markdown, Text(operation.Body, "summary"));
        AppendParagraph(markdown, Text(operation.Body, "description"));

        if (operation.Body.TryGetProperty("parameters", out var parameters) && parameters.GetArrayLength() > 0)
        {
            markdown.AppendLine("| Parameter | In | Type | Required | Description |");
            markdown.AppendLine("|---|---|---|---|---|");
            foreach (var parameter in parameters.EnumerateArray())
            {
                var schema = parameter.TryGetProperty("schema", out var parameterSchema) ? parameterSchema : default;
                var required = parameter.TryGetProperty("required", out var isRequired) && isRequired.GetBoolean() ? "yes" : "no";
                markdown.AppendLine($"| `{parameter.GetProperty("name").GetString()}` | {parameter.GetProperty("in").GetString()} | {Cell(TypeOf(schema))} | {required} | {Cell(Text(parameter, "description"))} |");
            }

            markdown.AppendLine();
        }

        if (operation.Body.TryGetProperty("requestBody", out var requestBody) && requestBody.TryGetProperty("content", out var content))
        {
            foreach (var media in content.EnumerateObject())
            {
                markdown.AppendLine($"Body (`{media.Name}`): {TypeOf(media.Value.GetProperty("schema"))}");
                markdown.AppendLine();
            }
        }

        if (operation.Body.TryGetProperty("responses", out var responses))
        {
            markdown.AppendLine("| Answer | Body |");
            markdown.AppendLine("|---|---|");
            foreach (var response in responses.EnumerateObject().OrderBy(response => response.Name, StringComparer.Ordinal))
            {
                var body = response.Value.TryGetProperty("content", out var responseContent)
                    ? string.Join(", ", responseContent.EnumerateObject().Select(media => TypeOf(media.Value.GetProperty("schema"))))
                    : "none";
                markdown.AppendLine($"| `{response.Name}` | {Cell(body)} |");
            }

            markdown.AppendLine();
        }
    }

    private static void RenderType(StringBuilder markdown, string name, JsonElement schema)
    {
        markdown.AppendLine($"### {name}");
        markdown.AppendLine();
        AppendParagraph(markdown, Text(schema, "description"));

        if (schema.TryGetProperty("enum", out var values))
        {
            var names = schema.TryGetProperty("x-enumNames", out var enumNames) ? enumNames.EnumerateArray().Select(value => value.GetString() ?? string.Empty).ToList() : [];
            markdown.AppendLine("| Value | Meaning |");
            markdown.AppendLine("|---|---|");
            var index = 0;
            foreach (var value in values.EnumerateArray())
            {
                var meaning = index < names.Count ? names[index] : string.Empty;
                markdown.AppendLine($"| `{value.GetRawText().Trim('"')}` | {Cell(meaning)} |");
                index++;
            }

            markdown.AppendLine();
            return;
        }

        if (!schema.TryGetProperty("properties", out var properties))
        {
            markdown.AppendLine($"Type: {TypeOf(schema)}");
            markdown.AppendLine();
            return;
        }

        var required = schema.TryGetProperty("required", out var requiredList)
            ? requiredList.EnumerateArray().Select(field => field.GetString()).ToHashSet(StringComparer.Ordinal)
            : [];

        markdown.AppendLine("| Field | Type | Required | Description |");
        markdown.AppendLine("|---|---|---|---|");
        foreach (var property in properties.EnumerateObject())
        {
            markdown.AppendLine($"| `{property.Name}` | {Cell(TypeOf(property.Value))} | {(required.Contains(property.Name) ? "yes" : "no")} | {Cell(Text(property.Value, "description"))} |");
        }

        markdown.AppendLine();
    }

    /// <summary>A short, readable type: a link for a reference, the TypeID prefix for an id, the values of a closed set.</summary>
    internal static string TypeOf(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return "any";
        }

        var nullable = schema.TryGetProperty("nullable", out var isNullable) && isNullable.GetBoolean() ? ", nullable" : string.Empty;

        if (schema.TryGetProperty("$ref", out var reference))
        {
            return Link(reference.GetString() ?? string.Empty) + nullable;
        }

        foreach (var composite in new[] { "oneOf", "allOf", "anyOf" })
        {
            if (schema.TryGetProperty(composite, out var parts))
            {
                return string.Join(" or ", parts.EnumerateArray().Select(TypeOf)) + nullable;
            }
        }

        var type = schema.TryGetProperty("type", out var typeName) ? typeName.GetString() : "object";
        if (type == "array")
        {
            return "array of " + TypeOf(schema.TryGetProperty("items", out var items) ? items : default) + nullable;
        }

        if (schema.TryGetProperty("enum", out var values) && type == "string")
        {
            var listed = values.EnumerateArray().Select(value => $"`{value.GetString()}`").ToList();
            return (listed.Count == 1 ? $"string, always {listed[0]}" : "string, one of " + string.Join(", ", listed)) + nullable;
        }

        if (type == "string" && schema.TryGetProperty("pattern", out var pattern) && Prefix(pattern.GetString()) is { } prefix)
        {
            return $"id (`{prefix}_…`)" + nullable;
        }

        var format = schema.TryGetProperty("format", out var formatName) ? formatName.GetString() : null;
        var readable = (type, format) switch
        {
            ("string", "date-time") => "timestamp",
            ("string", "date") => "date",
            ("string", "binary") => "file",
            ("string", "guid") or ("string", "uuid") => "uuid",
            ("integer", _) => "integer",
            ("number", _) => "number",
            ("boolean", _) => "boolean",
            _ => type
        };

        return readable + nullable;
    }

    private static string? Prefix(string? pattern)
    {
        var match = Regex.Match(pattern ?? string.Empty, "^\\^([a-z]+)_");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string Link(string reference)
    {
        var name = reference[(reference.LastIndexOf('/') + 1)..];
        return $"[{name}](#{Anchor(name)})";
    }

    private static string Scopes(JsonElement operation)
    {
        return operation.TryGetProperty("x-thrivea-scopes", out var scopes)
            ? string.Join(", ", scopes.EnumerateArray().Select(scope => $"`{scope.GetString()}`"))
            : "see the resource page";
    }

    // GitHub's heading anchors: lower case, spaces to hyphens, everything but letters, digits, hyphens and underscores dropped.
    internal static string Anchor(string heading)
    {
        var lower = heading.ToLower(CultureInfo.InvariantCulture).Replace(' ', '-');
        return Regex.Replace(lower, "[^a-z0-9_-]", string.Empty);
    }

    private static string Text(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        // XML comments arrive with their indentation; a paragraph break stays, the rest of the white space collapses.
        var paragraphs = Regex.Split(value.GetString()!.Trim(), "\\n\\s*\\n").Select(paragraph => Regex.Replace(paragraph, "\\s+", " ").Trim());
        return string.Join("\n\n", paragraphs.Where(paragraph => paragraph.Length > 0));
    }

    private static string Cell(string text)
    {
        return text.Replace("|", "\\|", StringComparison.Ordinal).Replace("\n\n", "<br><br>", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
    }

    private static void AppendParagraph(StringBuilder markdown, string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        markdown.AppendLine(text);
        markdown.AppendLine();
    }
}
