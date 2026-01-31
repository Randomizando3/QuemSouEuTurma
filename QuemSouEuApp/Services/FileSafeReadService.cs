namespace QuemSouEuApp.Services;

public static class FileSafeReadService
{
    /// <summary>
    /// Lê bytes do arquivo com retry e FileShare.ReadWrite (para contornar locks temporários).
    /// </summary>
    public static async Task<byte[]> ReadAllBytesWithRetryAsync(
        string path,
        int maxAttempts = 12,
        int delayMs = 120)
    {
        Exception? last = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // ✅ abre com compartilhamento para reduzir falha por lock
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var buffer = new byte[fs.Length];
                var read = 0;

                while (read < buffer.Length)
                {
                    var n = await fs.ReadAsync(buffer.AsMemory(read, buffer.Length - read));
                    if (n == 0) break;
                    read += n;
                }

                if (read == buffer.Length)
                    return buffer;

                // se leu menos, devolve o que leu
                return buffer.Take(read).ToArray();
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delayMs);
            }
        }

        throw new IOException($"Não foi possível ler o arquivo (lock persistente): {path}", last);
    }
}
