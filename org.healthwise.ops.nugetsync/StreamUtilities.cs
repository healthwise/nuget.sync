using System.IO;
using System.Threading.Tasks;

namespace org.healthwise.ops.nugetsync
{
    public static class StreamUtilities
    {
        public static async Task<Stream> MakeSeekable(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
                return stream;
            }

            return await MakeMemoryCopy(stream);
        }
        
        public static async Task<Stream> MakeMemoryCopy(Stream stream)
        {
            var streamCopy = new MemoryStream();
            await stream.CopyToAsync(streamCopy);
            streamCopy.Position = 0;
            return streamCopy;
        }
    }
}