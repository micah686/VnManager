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
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using VnManager.MetadataProviders.Vndb;
using System.Windows.Media;
using VndbSharp.Models.Common;
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
            if (maxPixels < 80)
            {
                maxPixels = 80;
            }
            //const int maxPixels = 150;
            if (stream == null)
            {
                return null;
            }
            if (stream.Length < 20)
            {
                return null; //memory streams for an image should be big, this should prevent streams with only a few bytes
            } 

            Image originalImg = Image.FromStream(stream);
            if (originalImg == null)
            {
                return null;
            }
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
                App.Logger.Warning(e, "Failed to get create bitmap from path");
                throw;
            }
        }

        /// <summary>
        /// Creates a BitmapSource from a specified stream. If the stream is too small, it creates a blank/empty Image
        /// </summary>
        /// <param name="stream">Stream of the image</param>
        /// <returns></returns>
        public static BitmapSource CreateBitmapFromStream(Stream stream)
        {
            try
            {
                if(stream== null)
                {
                    return CreateEmptyBitmapImage();
                }
                if (stream.Length > 20)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = stream;
                    img.EndInit();
                    img.Freeze();
                    stream.Dispose();
                    return img;
                }
                else
                {
                    return CreateEmptyBitmapImage();
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to get create bitmap from stream");
                throw;
            }
        }

        /// <summary>
        /// Download a list of images, and generate thumbnails as well
        /// </summary>
        /// <param name="imageList">List of Images (Uri and IsNsfw)</param>
        /// <param name="imageDirectory">Directory to save the images in</param>
        /// <returns></returns>
        public static async Task DownloadImagesWithThumbnailsAsync(IEnumerable<BindingImage> imageList, string imageDirectory)
        {
            try
            {
                var images = imageList.ToArray();
                if (!images.Any())
                {
                    return;
                }
                using (var client = new WebClient())
                {
                    if (!Directory.Exists($@"{imageDirectory}\thumbs\"))
                    {
                        Directory.CreateDirectory($@"{imageDirectory}\thumbs\");
                    }
                    
                    foreach (var screen in images)
                    {
                        if (screen.ImageLink == null || string.IsNullOrEmpty(screen.ImageLink))
                        {
                            continue;
                        }
                        var uri = new Uri(screen.ImageLink);
                        var imagePath = $@"{imageDirectory}\{Path.GetFileName(screen.ImageLink)}";
                        var thumbPath = $@"{imageDirectory}\thumbs\{Path.GetFileName(screen.ImageLink)}";

                        if(File.Exists(imagePath))
                        {
                            continue;
                        }

                        
                        var imageStream = new MemoryStream(await client.DownloadDataTaskAsync(uri));
                        var thumbImg = GetThumbnailImage(imageStream,0);
                        if (thumbImg == null)
                        {
                            continue;
                        }
                        if (NsfwHelper.RawRatingIsNsfw(screen.Rating))
                        {

                            Secure.EncStream(imageStream, imagePath);
                            var thumbStream = new MemoryStream();
                            thumbImg.Save(thumbStream, ImageFormat.Jpeg);
                            Secure.EncStream(thumbStream, thumbPath);
                            await thumbStream.DisposeAsync();
                        }
                        else
                        {
                            Image.FromStream(imageStream).Save(imagePath);
                            thumbImg.Save(thumbPath);
                            await imageStream.DisposeAsync();
                        }
                        await imageStream.DisposeAsync();
                    }
                }
                
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Failed to download images with thumbnails");
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
                if (string.IsNullOrEmpty(uri.AbsoluteUri))
                {
                    return;
                }
                using var client = new WebClient();
                if (isNsfw)
                {

                    byte[] imageBytes = await client.DownloadDataTaskAsync(uri);
                    var memStream = new MemoryStream(imageBytes);
                    if (memStream.Length < 1)
                    {
                        return;
                    }
                    Secure.EncStream(memStream, path);
                    await memStream.DisposeAsync();
                }
                else
                {
                    await client.DownloadFileTaskAsync(uri, path);
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
                var bitmapPath = $@"{App.ExecutableDirPath}\Resources\placeholders\empty_bitmap.png";
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
            {
                return CreateEmptyBitmapImage();
            }
            BitmapSource bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                sysIcon.Handle, System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            sysIcon.Dispose();
            return bmpSrc;
        }

        public static bool IsValidImage(string path)
        {
            try
            {
                ImageFormat img = Image.FromFile(path).RawFormat;
                return img.Equals(ImageFormat.Jpeg) || img.Equals(ImageFormat.Png);
            }
            catch(Exception)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Converts a BitmapSource into a Bitmap
        /// Used from: https://gist.github.com/nashby/916300
        /// </summary>
        /// <param name="source">BitmapSource of an image</param>
        /// <returns></returns>
        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap
            (
                source.PixelWidth,
                source.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            source.CopyPixels
            (
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }
        /// <summary>
        /// Converts a Bitmap to BitmapSource
        /// Used from: https://gist.github.com/nashby/916300
        /// </summary>
        /// <param name="bitmap">Bitmap of an Image</param>
        /// <returns></returns>
        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bitmapSource;
        }


        public static BitmapSource BlurImage(BitmapSource bs, int radialWeight)
        {
            var bi = GetBitmap(bs);
            var blurImg = new GaussianBlur(bi).Process(radialWeight);
            var blurBitmapSource = GetBitmapSource(blurImg);
            return blurBitmapSource;
        }
    }

    /// <summary>
    /// Custom class for dealing with properties of an image
    /// </summary>
    public class BindingImage
    {
        public string ImageLink { get; set; }
        public bool IsNsfw { get; set; }
        public ImageRating Rating { get; set; }
        public BitmapSource Image { get; set; }
    }
}
