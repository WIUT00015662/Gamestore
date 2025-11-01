namespace Gamestore.Api.Configuration;

public static class DotEnvLoader
{
    public static void Load(params string[] candidatePaths)
    {
        foreach (var path in candidatePaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                {
                    continue;
                }

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                Environment.SetEnvironmentVariable(key, value);
            }

            break;
        }
    }
}
