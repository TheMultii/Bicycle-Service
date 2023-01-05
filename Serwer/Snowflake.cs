namespace Serwer {
    public class Snowflake {
        private static readonly DateTime epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly long initialTime = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
        private static long lastTime = initialTime;
        private static readonly object syncRoot = new object();

        public static long Next() {
            lock (syncRoot) {
                long currentTime = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
                if (currentTime > lastTime) {
                    lastTime = currentTime;
                } else {
                    currentTime = ++lastTime;
                }

                return currentTime << 22;
            }
        }
    }
}
