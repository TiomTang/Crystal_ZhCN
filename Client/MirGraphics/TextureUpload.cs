using System.Drawing.Imaging;
using SlimDX.Direct3D9;

namespace Client.MirGraphics
{
    // CPU-owned pixels and pitch-aware staging; never retain a pointer after UnlockRectangle.
    internal static class TextureUpload
    {
        internal static Texture FromBytes(int width, int height, byte[] pixels)
        {
            if (pixels.Length != checked(width * height * 4))
                throw new InvalidDataException("Invalid BGRA texture length.");
            using var staging = new Texture(DXManager.Device, width, height, 1, Usage.None,
                Format.A8R8G8B8, Pool.SystemMemory);
            var data = staging.LockRectangle(0, LockFlags.None);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    data.Data.Position = (long)y * data.Pitch;
                    data.Data.Write(pixels, y * width * 4, width * 4);
                }
            }
            finally { staging.UnlockRectangle(0); data.Data.Dispose(); }
            var result = new Texture(DXManager.Device, width, height, 1, Usage.None,
                Format.A8R8G8B8, DXManager.Device is DeviceEx ? Pool.Default : Pool.Managed);
            try
            {
                if (DXManager.Device is DeviceEx) DXManager.Device.UpdateTexture(staging, result);
                else
                {
                    var destination = result.LockRectangle(0, LockFlags.None);
                    try
                    {
                        for (int y = 0; y < height; y++)
                        {
                            destination.Data.Position = (long)y * destination.Pitch;
                            destination.Data.Write(pixels, y * width * 4, width * 4);
                        }
                    }
                    finally { result.UnlockRectangle(0); destination.Data.Dispose(); }
                }
                return result;
            }
            catch { result.Dispose(); throw; }
        }

        internal static Texture Paint(int width, int height, Action<Bitmap> paint)
        {
            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            paint(bitmap);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] pixels = new byte[checked(width * height * 4)];
                for (int y = 0; y < height; y++)
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * width * 4, width * 4);
                return FromBytes(width, height, pixels);
            }
            finally { bitmap.UnlockBits(data); }
        }
    }
}
