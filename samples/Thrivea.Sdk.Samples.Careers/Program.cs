// A careers website talking to Thrivea, as a CMS would: find or create the job behind an ad, then send an application.
//
//   export THRIVEA_API_KEY=tk_live_…
//   dotnet run -- sync   cms-42 "Frontend Engineer" onsite
//   dotnet run -- apply  cms-42 Ana Anić ana@example.com [path/to/cv.pdf] [#frontend-engineer #novi-sad]
//   dotnet run -- list
using Thrivea;
using Thrivea.Errors;

var apiKey = Environment.GetEnvironmentVariable("THRIVEA_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Set THRIVEA_API_KEY to a Thrivea API key (tk_live_…).");
    return 2;
}

var options = new ThriveaClientOptions { ApiKey = apiKey };
var baseUrl = Environment.GetEnvironmentVariable("THRIVEA_BASE_URL");
if (!string.IsNullOrWhiteSpace(baseUrl))
{
    options.BaseUrl = baseUrl;
}

using var client = new ThriveaClient(options);

try
{
    switch (args.Length > 0 ? args[0] : "list")
    {
        case "list":
            await foreach (var job in client.JobOpenings.ListAutoPagingAsync())
            {
                Console.WriteLine($"{job.Id}  {job.Title,-40} {job.Status,-8} {job.WorkplaceType,-7} published={job.Published} external_ref={job.ExternalRef}");
            }

            return 0;

        case "sync":
            {
                // The CMS stores the id it gets back on the ad; every application from that ad is sent with it.
                var job = await FindOrCreateAsync(client, externalRef: args[1], title: args[2], workplaceType: args.Length > 3 ? args[3] : WorkplaceType.Onsite);
                Console.WriteLine($"{job.Id}  {job.Title} ({job.Status}, external_ref={job.ExternalRef})");
                return 0;
            }

        case "apply":
            {
                var job = await FindOrCreateAsync(client, externalRef: args[1], title: args[1], workplaceType: WorkplaceType.Onsite);
                var cvPath = args.Length > 5 && File.Exists(args[5]) ? args[5] : null;
                var tags = args.Skip(cvPath is null ? 5 : 6).ToList();

                var application = await client.CandidateApplications.CreateAsync(
                    new CandidateApplicationCreateOptions
                    {
                        JobOpeningId = job.Id,
                        FirstName = args[2],
                        LastName = args[3],
                        Email = args[4],
                        CoverNote = "Sent from the Thrivea.Sdk sample.",
                        Tags = tags,
                        ExternalRef = $"sample-{Guid.NewGuid():N}",
                        Cv = cvPath is null ? null : FileUpload.FromPath(cvPath)
                    },
                    new RequestOptions { IdempotencyKey = Guid.NewGuid().ToString() });

                Console.WriteLine($"{application.Id}  candidate={application.CandidateId} job={application.JobOpeningId} has_cv={application.HasCv}");
                return 0;
            }

        default:
            Console.Error.WriteLine("Commands: list | sync <external_ref> <title> [onsite|remote|hybrid] | apply <external_ref> <first> <last> <email> [cv] [#tags…]");
            return 2;
    }
}
catch (ThriveaPermissionException exception)
{
    Console.Error.WriteLine($"Not allowed: {exception.Message}");
    return 1;
}
catch (ThriveaException exception)
{
    Console.Error.WriteLine($"Thrivea said {(int?)exception.StatusCode} {exception.Error?.Type}: {exception.Message}");
    return 1;
}

static async Task<JobOpening> FindOrCreateAsync(IThriveaClient client, string externalRef, string title, string workplaceType)
{
    await foreach (var job in client.JobOpenings.ListAutoPagingAsync(new JobOpeningListOptions { Status = JobOpeningStatus.Open }))
    {
        if (job.ExternalRef == externalRef)
        {
            return job;
        }
    }

    // Idempotent by external_ref: sending the same one twice returns the job already created.
    return await client.JobOpenings.CreateAsync(new JobOpeningCreateOptions { Title = title, WorkplaceType = workplaceType, ExternalRef = externalRef });
}
