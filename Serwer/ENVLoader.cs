namespace Serwer {
    public static class ENVLoader {
        public static void Load(string FilePath) {
            if (!File.Exists(FilePath)) {
                throw new FileNotFoundException("File not found", FilePath);
            }
            foreach (string line in File.ReadAllLines(FilePath)) {
                string[] split = line.Split("=");
                if (split.Length != 2) {
                    continue;
                }
                Environment.SetEnvironmentVariable(split[0], split[1]);
            }
        }
        public static string GetString(string Key) {
            return Environment.GetEnvironmentVariable(Key) ?? throw new Exception("Environment variable not found");
        }
    }
}
