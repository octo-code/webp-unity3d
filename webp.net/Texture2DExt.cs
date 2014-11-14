
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

using WebP.Extern;

namespace WebP
{
    public static class Texture2DExt 
    {
        public static unsafe void LoadWebP(this Texture2D lTexture2D, byte[] lData, out Error lError)
        {
            lError = 0;
            byte[] lRawData = null;

            fixed (byte* lDataPtr = lData)
            {
                int lWidth = 0, lHeight = 0, lLength = lData.Length;

                if (NativeBindings.WebPGetInfo((IntPtr)lDataPtr, (UIntPtr)lLength, ref lWidth, ref lHeight) == 0)
                {
                    lError = Error.InvalidHeader;
                    throw new Exception("Invalid WebP header detected");
                }

                try
                {
                    lRawData = new byte[lWidth * lHeight * 4];
                    fixed (byte* lRawDataPtr = lRawData)
                    {
                        IntPtr result = NativeBindings.WebPDecodeRGBAInto((IntPtr)lDataPtr, (UIntPtr)lLength, (IntPtr)lRawDataPtr, (UIntPtr)(4 * lWidth * lHeight), 4 * lWidth);
                        if ((IntPtr)lRawDataPtr != result)
                        {
                            lError = Error.DecodingError;
                            throw new Exception("Failed to decode WebP image with error " + (long)result);
                        }
                    }
                    lError = Error.Success;
                }
                finally
                {
                }
            }

            if (lError == Error.Success)
            {
                lTexture2D.LoadRawTextureData(lRawData);
                lTexture2D.Apply();
            }
        }

        public static unsafe byte[] EncodeWebP(this Texture2D lTexture2D, out Error lError)
        {
            lError = 0;
            return null;
        }

        /*
        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="quality"></param>
        /// <param name="noAlpha"></param>
        public void Encode(Bitmap from, Stream to, float quality, bool noAlpha = false)
        {
            IntPtr result;
            long length;

            Encode(from, quality, noAlpha, out result, out length);
            try
            {
                byte[] buffer = new byte[4096];
                for (int i = 0; i < length; i += buffer.Length)
                {
                    int used = (int)Math.Min((int)buffer.Length, length - i);
                    Marshal.Copy((IntPtr)((long)result + i), buffer, 0, used);
                    to.Write(buffer, 0, used);
                }
            }
            finally
            {
                NativeBindings.WebPSafeFree(result);
            }

        }
        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="quality"></param>
        /// <param name="noAlpha"></param>
        /// <param name="result"></param>
        /// <param name="length"></param>
        public void Encode(Bitmap b, float quality, bool noAlpha, out IntPtr result, out long length)
        {
            if (quality < -1) quality = -1;
            if (quality > 100) quality = 100;
            int w = b.Width;
            int h = b.Height;
            var bd = b.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, b.PixelFormat);
            try
            {
                result = IntPtr.Zero;

                if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb && !noAlpha)
                {
                    if (quality == -1) length = (long)NativeBindings.WebPEncodeLosslessBGRA(bd.Scan0, w, h, bd.Stride, ref result);
                    else length = (long)NativeBindings.WebPEncodeBGRA(bd.Scan0, w, h, bd.Stride, quality, ref result);
                }
                else if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb || (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb && noAlpha))
                {
                    if (quality == -1) length = (long)NativeBindings.WebPEncodeLosslessBGR(bd.Scan0, w, h, bd.Stride, ref result);
                    else length = (long)NativeBindings.WebPEncodeBGR(bd.Scan0, w, h, bd.Stride, quality, ref result);
                }
                else
                {
                    throw new NotSupportedException("Only Format32bppArgb and Format32bppRgb bitmaps are supported");
                }
                if (length == 0) throw new Exception("WebP encode failed!");

            }
            finally
            {
                b.UnlockBits(bd);
            }
        }*/
    }
}
