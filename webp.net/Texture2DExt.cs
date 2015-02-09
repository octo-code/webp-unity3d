
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

using WebP.Extern;

namespace WebP
{
    /// <summary>
    /// 
    /// </summary>
    public static class Texture2DExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lData"></param>
        /// <param name="lError"></param>
        /// <returns></returns>
        public static unsafe Texture2D CreateTexture2DFromWebP(byte[] lData, bool lMipmaps, bool lLinear, out Error lError)
        {
            lError = 0;
            byte[] lRawData = null;
            Texture2D lTexture2D = null;
            int lWidth = 0, lHeight = 0, lLength = lData.Length;

            fixed (byte* lDataPtr = lData)
            {
                try
                {
                    if (NativeBindings.WebPGetInfo((IntPtr)lDataPtr, (UIntPtr)lLength, ref lWidth, ref lHeight) == 0)
                    {
                        lError = Error.InvalidHeader;
                        throw new Exception("Invalid WebP header detected");
                    }

                    lRawData = new byte[lWidth * lHeight * 4];
                    fixed (byte* lRawDataPtr = lRawData)
                    {
                        int lStride = 4 * lWidth;
                        byte* lTmpDataPtr = lRawDataPtr + (lHeight - 1) * lStride;

                        IntPtr result = NativeBindings.WebPDecodeRGBAInto((IntPtr)lDataPtr, (UIntPtr)lLength, (IntPtr)lTmpDataPtr, (UIntPtr)(4 * lWidth * lHeight), -lStride);
                        if ((IntPtr)lTmpDataPtr != result)
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
                lTexture2D = new Texture2D(lWidth, lHeight, TextureFormat.RGBA32, false, false);
                lTexture2D.LoadRawTextureData(lRawData);
                lTexture2D.Apply();
            }

            return lTexture2D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lTexture2D"></param>
        /// <param name="lData"></param>
        /// <param name="lError"></param>
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
                        int lStride = 4 * lWidth;
                        byte* lTmpDataPtr = lRawDataPtr + (lHeight - 1) * lStride;

                        IntPtr result = NativeBindings.WebPDecodeRGBAInto((IntPtr)lDataPtr, (UIntPtr)lLength, (IntPtr)lTmpDataPtr, (UIntPtr)(4 * lWidth * lHeight), -lStride);
                        if ((IntPtr)lTmpDataPtr != result)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lTexture2D"></param>
        /// <param name="lError"></param>
        /// <returns></returns>
        public static unsafe byte[] EncodeToWebP(this Texture2D lTexture2D, float lQuality, out Error lError)
        {
            lError = 0;

            if (lQuality < -1)  lQuality = -1;
            if (lQuality > 100) lQuality = 100;

            Color32[] lRawColorData = lTexture2D.GetPixels32();
            int lWidth  = lTexture2D.width;
            int lHeight = lTexture2D.height;

            IntPtr lResult = IntPtr.Zero;

            GCHandle lPinnedArray = GCHandle.Alloc(lRawColorData, GCHandleType.Pinned);
            IntPtr lRawDataPtr = lPinnedArray.AddrOfPinnedObject();

            byte[] lOutputBuffer = null;

            try
            {
                int lLength;

                if (lQuality == -1)
                {
                    lLength = (int)NativeBindings.WebPEncodeLosslessRGBA(lRawDataPtr, lWidth, lHeight, 4 * lWidth, ref lResult);
                }
                else
                {
                    lLength = (int)NativeBindings.WebPEncodeRGBA(lRawDataPtr, lWidth, lHeight, 4 * lWidth, lQuality, ref lResult);
                }

                if (lLength == 0)
                {
                    throw new Exception("WebP encode failed!");
                }

                lOutputBuffer = new byte[lLength];
                Marshal.Copy(lResult, lOutputBuffer, 0, lLength);
            }
            finally
            {
                NativeBindings.WebPSafeFree(lResult);
            }

            lPinnedArray.Free();

            return lOutputBuffer;
        }
    }
}
