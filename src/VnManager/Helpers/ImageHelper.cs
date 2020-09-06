using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VnManager.MetadataProviders.Vndb;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;

namespace VnManager.Helpers
{
    public static class ImageHelper
    {

        /// <summary>
        /// Creates a thumbnail image from a specified image stream
        /// </summary>
        /// <param name="stream">MemoryStream of Image</param>
        /// <param name="maxPixels">Factor of original size. If 0, defaults to 80</param>
        /// <returns></returns>
        public static Image GetThumbnailImage(Stream stream, int maxPixels)
        {
            if (maxPixels < 80) maxPixels = 80;
            //const int maxPixels = 150;
            if (stream == null) return null;
            if (stream.Length < 20) return null; //memory streams for an image should be big, this should prevent streams with only a few bytes

            Image originalImg = Image.FromStream(stream);
            if (originalImg == null) return null;
            //get thumbnail size
            double originalWidth = originalImg.Width;
            double originalHeight = originalImg.Height;
            var factor = originalWidth > originalHeight
                ? (double) maxPixels / originalWidth
                : (double) maxPixels / originalHeight;
            Size thumbnailSize = new Size((int)(originalWidth * factor), (int)(originalHeight * factor));


            Bitmap bitmap = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
            Graphics g = Graphics.FromImage((Image)bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(originalImg, 0, 0, thumbnailSize.Width, thumbnailSize.Height);

            var img = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.DontCare);
            bitmap.Dispose();
            return img;
        }

        /// <summary>
        /// Creates a BitmapSource from a specified path. If the file doesn't exist, it creates a blank/empty Image
        /// </summary>
        /// <param name="path">Path to image</param>
        /// <returns></returns>
        public static BitmapSource CreateBitmapFromPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = fileStream;
                    img.EndInit();
                    img.Freeze();
                    fileStream.Dispose();
                    return img;
                }
                else
                {
                    return CreateEmptyBitmapImage();
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to get cover image");
                throw;
            }
        }

        /// <summary>
        /// Download a list of images, and generate thumbnails as well
        /// </summary>
        /// <param name="imageList">List of Images (Uri and IsNsfw)</param>
        /// <param name="imageDirectory">Directory to save the images in</param>
        /// <returns></returns>
        public static async Task DownloadImagesWithThumbnailsAsync(IEnumerable<ScreenShot> imageList, string imageDirectory)
        {
            try
            {
                if (imageList == null || !imageList.Any()) return;
                using (var client = new WebClient())
                {
                    foreach (var screen in imageList)
                    {
                        if (screen.Uri == null || string.IsNullOrEmpty(screen.Uri.AbsoluteUri)) continue;
                        var imageDir = $@"{imageDirectory}\{Path.GetFileName(screen.Uri.AbsoluteUri)}";
                        var thumbDir = $@"{imageDirectory}\thumbs\{Path.GetFileName(screen.Uri.AbsoluteUri)}";

                        if(File.Exists(imageDir))continue;

                        var imageStream = new MemoryStream(await client.DownloadDataTaskAsync(screen.Uri.AbsoluteUri));
                        var thumbImg = GetThumbnailImage(imageStream,0);
                        if (thumbImg == null) continue;
                        if (screen.IsNsfw && App.UserSettings.IsVisibleSavedNsfwContent == false)
                        {

                            Secure.EncStream(imageStream, imageDir);
                            var thumbStream = new MemoryStream();
                            thumbImg.Save(thumbStream, ImageFormat.Jpeg);
                            Secure.EncStream(thumbStream, thumbDir);
                            await thumbStream.DisposeAsync();
                        }
                        else
                        {
                            Image.FromStream(imageStream).Save(imageDir);
                            thumbImg.Save(thumbDir);
                            await imageStream.DisposeAsync();
                        }
                        await imageStream.DisposeAsync();
                    }
                }
                
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download images");
            }
            
        }



        /// <summary>
        /// Downloads an an image from the specified Uri
        /// </summary>
        /// <param name="uri">URI/URL of the image to be downloaded</param>
        /// <param name="isNsfw">Is the image Nsfw</param>
        /// <param name="path">Directory and filename of where the image will be saved</param>
        /// <returns></returns>
        public static async Task DownloadImageAsync(Uri uri, bool isNsfw, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(uri.AbsoluteUri)) return;
                using (var client = new WebClient())
                {
                    if (isNsfw && App.UserSettings.IsVisibleSavedNsfwContent == false)
                    {

                        byte[] imageBytes = await client.DownloadDataTaskAsync(uri);
                        var memStream = new MemoryStream(imageBytes);
                        if (memStream.Length < 1) return;
                        Secure.EncStream(memStream, path);
                        await memStream.DisposeAsync();
                    }
                    else
                    {
                        await client.DownloadFileTaskAsync(uri, path);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex,"Failed to download image");
            }
        }


        /// <summary>
        /// Creates a BitmapSource from a blank image
        /// </summary>
        /// <returns></returns>
        public static BitmapSource CreateEmptyBitmapImage()
        {
            try
            {
                var bitmapPath = $@"{App.ExecutableDirPath}\Resources\Placeholders\empty_bitmap.png";
                BitmapSource bs = new BitmapImage(new Uri(bitmapPath));
                return bs;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to load empty image");
                throw;
            }
        }

        

        //TODO: Method not used yet
        public static BitmapSource CreateIcon(string path)
        {
            if (path == null)
            {
                return CreateEmptyBitmapImage();
            }
            Icon sysIcon = Icon.ExtractAssociatedIcon(path);
            if (sysIcon == null)
                return CreateEmptyBitmapImage();
            BitmapSource bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                sysIcon.Handle, System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            sysIcon.Dispose();
            return bmpSrc;
        }
        
    }
    public class ScreenShot
    {
        public Uri Uri { get; set; }
        public bool IsNsfw { get; set; }
    }
}
