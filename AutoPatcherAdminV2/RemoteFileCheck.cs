namespace AutoPatcherAdmin
{
    internal static class RemoteFileCheck
    {
        // Check the bytes served to clients, not the expanded size of gzip files.
        public static string? Check(string name, long length, long compressed, Func<string, long?> getLength)
        {
            if (length < 0 || compressed < 0) throw new InvalidDataException("Invalid manifest length.");
            string path = name.Replace('\\', '/');
            if (path != "PList.gz" && (compressed != length || compressed == 0)) path += ".gz";
            long expected = compressed == 0 ? length : compressed;
            long? actual = getLength(path);
            if (actual == null) return $"Remote file is missing: {path}";
            if (actual < 0) throw new IOException($"Remote length is unavailable: {path}");
            return actual == expected ? null : $"Remote size mismatch: {path}; expected {expected} bytes, actual {actual} bytes.";
        }
    }
}
