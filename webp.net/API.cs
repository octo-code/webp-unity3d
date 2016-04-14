
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
    public static class API
    {

		/// <summary>
		/// Gets dimensions from a webp format block of data.
		/// </summary>
		/// <param name="lData">L data.</param>
		/// <param name="lWidth">L width.</param>
		/// <param name="lHeight">L height.</param>
		public static unsafe void GetWebPDimensions(byte[] lData, out int lWidth, out int lHeight)
		{
			fixed (byte* lDataPtr = lData)
			{
				lWidth = 0;
				lHeight = 0;
				if (NativeBindings.WebPGetInfo((IntPtr)lDataPtr, (UIntPtr)lData.Length, ref lWidth, ref lHeight) == 0)
				{
					throw new Exception("Invalid WebP header detected");
				}
			}
		}

		/// <summary>
		/// Loads an image from webp into a byte array in RGBA format.
		/// </summary>
		/// <returns>The RGBA from web p.</returns>
		/// <param name="lData">L data.</param>
		/// <param name="lWidth">L width.</param>
		/// <param name="lHeight">L height.</param>
		/// <param name="lMipmaps">If set to <c>true</c> l mipmaps.</param>
		/// <param name="lError">L error.</param>
		/// <param name="scalingFunction">Scaling function.</param>
		public static unsafe Status DecodeWebP(byte[] lInput, ref int lWidth, ref int lHeight, ref WEBP_CSP_MODE lColorSpace,
                                               bool lMipmaps, out byte[] lOutput, bool lReducedColorRange = false, 
                                               bool lReducedScale = false)
		{
            Status lStatus = 0;
            int lLength = lInput.Length;
            int lBytesPerTexel = 4;

            fixed (byte* lDataPtr = lInput)
            {
                WebPDecoderConfig config = new WebPDecoderConfig();

                if (NativeBindings.WebPInitDecoderConfig(ref config) == 0)
                {
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
                }

                if (lReducedScale == true)
                {
                    lWidth /= 2;
                    lHeight /= 2;
                }

                // Set up decode options
                config.options.use_threads = 0;
                if (lReducedScale == true)
                {
                    config.options.use_scaling = 1;
                    config.options.scaled_width = lWidth;
                    config.options.scaled_height = lHeight;
                }

                // read the .webp input file information
                VP8StatusCode result = NativeBindings.WebPGetFeatures((IntPtr)lDataPtr, (UIntPtr)lLength, ref config.input);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new Exception(string.Format("Failed WebPGetFeatures with error {0}.", result.ToString()));
                }

                //  confirm colorspace.
                if (config.input.has_alpha > 0)
                {
                    if (lReducedColorRange == true)
                    {
                        lColorSpace = WEBP_CSP_MODE.MODE_RGBA_4444;
                        lBytesPerTexel = 2;
                    }
                    else
                    {
                        lColorSpace = WEBP_CSP_MODE.MODE_RGBA;
                        lBytesPerTexel = 4;
                    }
                }
                else
                {
                    if (lReducedColorRange == true)
                    {
                        lColorSpace = WEBP_CSP_MODE.MODE_RGB_565;
                        lBytesPerTexel = 2;
                    }
                    else
                    {
                        lColorSpace = WEBP_CSP_MODE.MODE_RGB;
                        lBytesPerTexel = 3;
                    }
                }

                // Bytes per texel can only be calculated at this point...
                int lStride = lBytesPerTexel * lWidth;

				// If mipmaps are requested we need to create 1/3 more memory for the mipmaps to be generated in.
                int lSize = lHeight * lStride;
				if (lMipmaps)   //  don't do this here..
				{   
                    //  bit shift instead of this crude approach.,...
                    lSize += Mathf.CeilToInt((float)lSize / 3.0f);
				}

                lOutput = new byte[lSize];
                fixed (byte* lOutputPtr = lOutput)
                {
                    // As we have to reverse the y order of the data, we pass through a negative stride and 
                    // pass through a pointer to the last line of the data.
                    byte* lTmpDataPtr = lOutputPtr + (lSize - lStride);

					// specify the output format
                    config.output.colorspace    = lColorSpace;
                    config.output.u.RGBA.rgba   = (IntPtr)(lTmpDataPtr);
					config.output.u.RGBA.stride = -lStride;
                    config.output.u.RGBA.size   = (UIntPtr)lSize;
					config.output.height        = lHeight;
					config.output.width         = lWidth;
					config.output.is_external_memory = 1;

					// Decode
					result = NativeBindings.WebPDecode((IntPtr)lDataPtr, (UIntPtr)lLength, ref config);
					if (result != VP8StatusCode.VP8_STATUS_OK)
					{
						throw new Exception(string.Format("Failed WebPDecode with error {0}.", result.ToString()));
					}
				}
                lStatus = Status.SUCCESS;
			}
            return lStatus;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lTexture2D"></param>
        /// <param name="lError"></param>
        /// <returns></returns>
        public static unsafe Status EncodeToWebP(this Texture2D lTexture2D, float lQuality, out byte[] lOutput)
        {
            Status lStatus = 0;

            if (lQuality < -1)  lQuality = -1;
            if (lQuality > 100) lQuality = 100;

            Color32[] lRawColorData = lTexture2D.GetPixels32();
            int lWidth  = lTexture2D.width;
            int lHeight = lTexture2D.height;

            IntPtr lResult = IntPtr.Zero;

            GCHandle lPinnedArray = GCHandle.Alloc(lRawColorData, GCHandleType.Pinned);
            IntPtr lRawDataPtr = lPinnedArray.AddrOfPinnedObject();

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

                lOutput = new byte[lLength];
                Marshal.Copy(lResult, lOutput, 0, lLength);
            }
            finally
            {
                NativeBindings.WebPSafeFree(lResult);
            }

            lPinnedArray.Free();

            return lStatus;
        }
    }
}
