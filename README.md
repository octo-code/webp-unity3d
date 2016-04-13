webp-unity3d
============

Still a work in progress

Note : https://github.com/imazen/libwebp-net/ is being used as a basis for the .net wrapper


Unity WebP API
==============

+ Needs to Decode the WebP Data into an Image Structure (that can later be used to create a Texture2D)
+ Needs to support Multiple ColorSpaces for Output Data
+ Supports Scaling of Image
+ Can pass Options to Decode / Encode Request
+ Mimic native API as closely as possible
+ Generate a Header Structure 
+ Needs to support Multi-Threading

+ Useful Scale / Crop Calculation can be done in an Image Support Class
+ Platform support : win x86, win x86_64, ios arm7, ios arm64, android arm7, android x86, osx x86, osx x86_64, linux x86, linux x86_64
