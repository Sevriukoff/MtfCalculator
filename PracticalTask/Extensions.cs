using System;
using _AVM.Library.Core;

namespace PracticalTask
{
    public static class Extensions
    {
        public static byte[,] ToArray(this Image._8bit img)
        {
            byte[,] result = new byte[img.Size.Width, img.Size.Height];
            
            for (int i = 0; i < img.Size.Width; i++)
            {
                for (int j = 0; j < img.Size.Height; j++)
                {
                    result[i, j] = img[i, j];
                }
            }

            return result;
        }
        
        public static int Count(this Image._8bit img, Func<byte, bool> predicate)
        {
            if (img == null)
                throw new ArgumentException();
            if (predicate == null)
                throw new ArgumentException();
            
            int num = 0;
            
            for (int i = 0; i < img.Size.Width; i++)
            {
                for (int j = 0; j < img.Size.Height; j++)
                {
                    if (predicate(img[i,j]))
                        checked { ++num; }
                }
            }
            
            return num;
        }
        
        public static int Count(this Image._8bit.Pixel[,] img, Func<byte, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentException();
            
            int num = 0;
            
            for (int i = 0; i < img.GetLength(0); i++)
            {
                for (int j = 0; j < img.GetLength(1); j++)
                {
                    if (predicate(img[i,j].Gray))
                        checked { ++num; }
                }
            }
            
            return num;
        }
        
        public static void CopyTo(this Image._8bit img, Image._8bit.Pixel[,] destination)
        {
            if (img == null)
                throw new ArgumentException();

            for (int i = 0; i < img.Size.Width; i++)
            {
                for (int j = 0; j < img.Size.Height; j++)
                {
                    destination[i, j].Gray = img[i, j];
                }
            }
            
        }
    }
}