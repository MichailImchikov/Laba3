using System.Globalization;

namespace Lab_Test
{
    internal static class DataTaskLoader
    {
        public static List<DataTask> LoadAll()
        {
            var result = new List<DataTask>();
            var task3 = FindTask3Directory();
            if (task3 == null) return result;
            foreach (var file in GetDataFilesFromTask3(task3))
            {

                LoadSingle(file, out var times, out var directiveTimes);
                result.Add(new DataTask(times, directiveTimes, file));

            }
            return result;
        }

        private static void LoadSingle(string fileName, out List<List<long>> times, out List<long> directiveTimes)
        {
            var tokens = Tokenize(File.ReadAllText(fileName));
            int idx = 0;
            int n = (int)ReadLong(tokens, ref idx);
            directiveTimes = new List<long>(new long[n + 1]);
            directiveTimes[0] = 0;
            for (int i = 0; i < n; i++)
            {
                directiveTimes[i + 1] = ReadLong(tokens, ref idx);
            }
            times = new List<List<long>>();
            for (int i = 0; i <= n; i++)
            {
                var row = new List<long>(n + 1);
                for (int j = 0; j <= n; j++)
                {
                    row.Add(ReadLong(tokens, ref idx));
                }
                times.Add(row);
            }
        }

        private static List<string> Tokenize(string input)
        {
            var list = new List<string>();
            using var sr = new StringReader(input);
            var sb = new System.Text.StringBuilder();
            while (true)
            {
                int ch = sr.Read();
                if (ch == -1)
                {
                    if (sb.Length > 0) list.Add(sb.ToString());
                    break;
                }
                if (char.IsWhiteSpace((char)ch))
                {
                    if (sb.Length > 0)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append((char)ch);
                }
            }
            return list;
        }

        private static long ReadLong(List<string> tokens, ref int idx)
        {
            if (idx >= tokens.Count) throw new InvalidDataException("Unexpected end of file");
            var s = tokens[idx++];
            if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v)) return v;
            throw new InvalidDataException($"Invalid integer: {s}");
        }

        private static string? FindTask3Directory()
        {
            var dir = AppContext.BaseDirectory;
            DirectoryInfo? current = new DirectoryInfo(dir);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "Task3");
                if (Directory.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            return null;
        }

        private static IEnumerable<string> GetDataFilesFromTask3(string task3Dir)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt", ".dat", ".data", ".in", ".inp" };
            var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".csproj", ".sln", ".md", ".json" };

            bool IsData(string path)
            {
                var ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext)) return true;
                if (excluded.Contains(ext)) return false;
                if (allowed.Contains(ext)) return true;
                return false;
            }

            var files = Directory.GetFiles(task3Dir).Where(IsData).ToList();
            if (files.Count == 0)
            {
                files = Directory.GetFiles(task3Dir, "*", SearchOption.AllDirectories).Where(IsData).ToList();
            }
            return files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
        }
    }
}
