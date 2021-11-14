using System.IO;
using System.IO.Compression;
using System.Text;

namespace RegenAoc
{
    public class StreamHelper
    {

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }


        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Stream CompressStream(Stream uncompressed)
        {
            var compressed = new MemoryStream();
            var gzipStream = new GZipStream(compressed, CompressionMode.Compress);

            while (true)
            {
                var buff = new byte[1024];
                var read = uncompressed.Read(buff, 0, buff.Length);
                if (read == 0)
                    break;
                gzipStream.Write(buff, 0, read);
            };
            // int read;
            // do
            // {
            //     var buff = new byte[1024];
            //     read = uncompressed.Read(buff, 0, buff.Length);
            //     gzipStream.Write(buff, 0, read);
            // } while (read != 0);
            gzipStream.Flush();
            compressed.Position = 0;
            return compressed;
        }
    }
}