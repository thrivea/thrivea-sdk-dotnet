namespace Thrivea;

/// <summary>A file to send with a request, such as a candidate's CV.</summary>
public sealed class FileUpload
{
    private readonly Stream? _stream;
    private byte[]? _bytes;

    /// <summary>A file read from <paramref name="content" />. The stream is read once, when the request is sent.</summary>
    public FileUpload(Stream content, string fileName, string contentType)
    {
        _stream = content ?? throw new ArgumentNullException(nameof(content));
        FileName = Require(fileName, nameof(fileName));
        ContentType = Require(contentType, nameof(contentType));
    }

    private FileUpload(byte[] bytes, string fileName, string contentType)
    {
        _bytes = bytes;
        FileName = Require(fileName, nameof(fileName));
        ContentType = Require(contentType, nameof(contentType));
    }

    /// <summary>The name the file is stored under, as the candidate named it.</summary>
    public string FileName { get; }

    /// <summary>The media type: <c>application/pdf</c>, a Word type, or <c>text/plain</c>.</summary>
    public string ContentType { get; }

    /// <summary>A file already in memory.</summary>
    public static FileUpload FromBytes(byte[] content, string fileName, string contentType)
    {
        return new FileUpload(content ?? throw new ArgumentNullException(nameof(content)), fileName, contentType);
    }

    /// <summary>A file on disk. The media type is taken from the extension when not given.</summary>
    public static FileUpload FromPath(string path, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("path is required.", nameof(path));
        }

        return new FileUpload(File.ReadAllBytes(path), Path.GetFileName(path), contentType ?? ContentTypeFor(path));
    }

    /// <summary>The media type a CV file usually has, by its extension.</summary>
    public static string ContentTypeFor(string fileName)
    {
        switch (Path.GetExtension(fileName).ToLowerInvariant())
        {
            case ".pdf":
                return "application/pdf";
            case ".doc":
                return "application/msword";
            case ".docx":
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case ".txt":
                return "text/plain";
            default:
                return "application/octet-stream";
        }
    }

    internal async Task<byte[]> ReadAsync(CancellationToken cancellationToken)
    {
        if (_bytes is not null)
        {
            return _bytes;
        }

        using var buffer = new MemoryStream();
#if NETSTANDARD2_0
        await _stream!.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);
#else
        await _stream!.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
#endif
        _bytes = buffer.ToArray();
        return _bytes;
    }

    private static string Require(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value;
    }
}
