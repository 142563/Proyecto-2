using System.Text;
using Npgsql;

var rawConnectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    Console.Error.WriteLine("Connection string missing. Pass as arg[0] or CONNECTION_STRING env var.");
    return 1;
}

var connectionString = NormalizeConnectionString(rawConnectionString);

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var schemaPath = Path.Combine(repoRoot, "db", "schema.sql");
var seedPath = Path.Combine(repoRoot, "db", "seed.sql");
var dataFixPath = Path.Combine(repoRoot, "db", "data_fix_consistency.sql");
var historyStatusFixPath = Path.Combine(repoRoot, "db", "data_fix_history_statuses.sql");

if (!File.Exists(schemaPath) || !File.Exists(seedPath))
{
    Console.Error.WriteLine($"Could not find schema or seed files at {schemaPath} / {seedPath}");
    return 1;
}

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

var scripts = new List<string> { schemaPath, seedPath };
if (File.Exists(dataFixPath))
{
    scripts.Add(dataFixPath);
}

if (File.Exists(historyStatusFixPath))
{
    scripts.Add(historyStatusFixPath);
}

foreach (var scriptPath in scripts)
{
    var sql = await File.ReadAllTextAsync(scriptPath, Encoding.UTF8);
    var scriptName = Path.GetFileName(scriptPath);

    await using var command = new NpgsqlCommand(sql, connection)
    {
        CommandTimeout = 120
    };

    Console.WriteLine($"Executing {scriptName} ...");
    await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Executed {scriptName}");
}

Console.WriteLine("Database bootstrap completed.");
return 0;

static string NormalizeConnectionString(string input)
{
    if (!input.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
        !input.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        return input;
    }

    var uri = new Uri(input);
    var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        Database = uri.AbsolutePath.Trim('/'),
    };

    var query = uri.Query.TrimStart('?');
    if (!string.IsNullOrWhiteSpace(query))
    {
        var parts = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(kv[0]);
            var value = Uri.UnescapeDataString(kv[1]);

            if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                builder["SSL Mode"] = value;
                continue;
            }

            if (key.Equals("channel_binding", StringComparison.OrdinalIgnoreCase))
            {
                builder["Channel Binding"] = value;
                continue;
            }

            builder[key] = value;
        }
    }

    return builder.ConnectionString;
}
