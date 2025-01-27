using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Diagnostics;

namespace TravelMemories.Utilities.Storage
{
    public interface IImageCompressService
    {
        MemoryStream CompressImage(IFormFile imageFile, JpegEncoder jpegOptions);
    }

    public class ImageCompressService : IImageCompressService
    {
        private ILogger<ImageCompressService> _logger;

        public ImageCompressService(ILogger<ImageCompressService> logger)
        {
            _logger = logger;
        }

        public MemoryStream CompressImage(IFormFile imageFile, JpegEncoder jpegOptions)
        {
            MemoryStream compressedStream = new MemoryStream();
            Stopwatch sw = Stopwatch.StartNew();

            using var imageData = Image.Load(imageFile.OpenReadStream());

            imageData.Save(compressedStream, jpegOptions);
            _logger.LogInformation($"Compressed {imageFile.FileName}, took {sw.ElapsedMilliseconds}ms");
            sw.Stop();
            // this is required because writing to a stream will set its position to end, and when we try to read it (for ex to upload to storage account),
            // it will read from the end so we need to set its position to 0
            compressedStream.Position = 0;

            return compressedStream;
        }
    }
}
