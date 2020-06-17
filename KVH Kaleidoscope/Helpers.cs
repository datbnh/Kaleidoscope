using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public static class Helpers
    {
        private const string ImageFileExtFilters = "Joint Photographic Experts Group|*.jpg" +
                        "|Portable Network Graphics|*.png" +
                        "|BMP|*.bmp" +
                        "|Graphics Interchange Format|*.gif" +
                        "|Exchangeable Image File|*.exif" +
                        "|Tag Image File Format|*.tiff";
        private const string AllImageFileExtFilter = "|All Picture Files|*.jpg;*.png;*.bmp;*.gif;*.exif;*.tiff|All Files|*.*";

        public static ImageFormat ToImageFormat(this string extension, ImageFormat defaultFormat)
        {
            switch (extension)
            {
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".png":
                    return ImageFormat.Png;
                case ".exif":
                    return ImageFormat.Exif;
                case ".tiff":
                    return ImageFormat.Tiff;
                default:
                    return defaultFormat;
            }
        }

        public static void SaveAsFile(this Image image, string filePath)
        {
            try
            {
                image.Save(filePath, Path.GetExtension(filePath).ToImageFormat(ImageFormat.Jpeg));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InitFiltersAsImageSaveFileDialog(this SaveFileDialog saveFileDialog)
        {
            saveFileDialog.Filter = ImageFileExtFilters;
            saveFileDialog.DefaultExt = "jpg";
            saveFileDialog.FilterIndex = 1;
        }

        public static void InitFiltersAsImageOpenFileDialog(this OpenFileDialog openFileDialog)
        {
            openFileDialog.Filter = ImageFileExtFilters + AllImageFileExtFilter;
            openFileDialog.FilterIndex = 7;
        }
    }
}
