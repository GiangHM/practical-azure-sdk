using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobSasTokenGenerationAf.Middlewares.RequestDecompression
{
    public class GzipDecompressionProvider : IDecompressionProvider
    {
        public string GetDecompression(string compression)
        {
            string res = string.Empty;
            var gZipBuffer = Convert.FromBase64String(compression);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);
                var buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }
                res = Encoding.UTF8.GetString(buffer);
            }
            return res;
        }
    }
}
