using System.IO;
using System.IO.Compression;
using System.Text;

public static class Utility
{
    public static string CompressString(string str)
    {
        byte[] data = Encoding.UTF8.GetBytes(str);
        using (var stream = new MemoryStream())
        {
            using (var gzip = new GZipStream(stream, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return System.Convert.ToBase64String(stream.ToArray());
        }
    }

    public static string DecompressString(string compressedStr)
    {
        byte[] data = System.Convert.FromBase64String(compressedStr);
        using (var input = new MemoryStream(data))
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }
    }
}