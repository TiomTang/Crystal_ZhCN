using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
namespace Launcher;

// DPAPI binds recovery metadata to the Windows account and installation path.
// It detects edited journal targets/progress; it is not remote patch authentication.
internal static class JournalProtection
{
    [StructLayout(LayoutKind.Sequential)] struct Blob { public int Length; public IntPtr Data; }
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptProtectData(ref Blob input, string description, ref Blob entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptUnprotectData(ref Blob input, IntPtr description, ref Blob entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);
    [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr pointer);
    public static byte[] Transform(byte[] data, string root, bool protect)
    {
        byte[] context = Encoding.UTF8.GetBytes("Mir2 transaction v1|" + Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)).ToUpperInvariant());
        var input = new Blob { Length = data.Length, Data = Marshal.AllocHGlobal(data.Length) };
        var entropy = new Blob { Length = context.Length, Data = Marshal.AllocHGlobal(context.Length) };
        Blob output = default;
        try
        {
            Marshal.Copy(data, 0, input.Data, data.Length); Marshal.Copy(context, 0, entropy.Data, context.Length);
            bool ok = protect ? CryptProtectData(ref input, "Mir2 update recovery", ref entropy, IntPtr.Zero, IntPtr.Zero, 1, out output) : CryptUnprotectData(ref input, IntPtr.Zero, ref entropy, IntPtr.Zero, IntPtr.Zero, 1, out output);
            if (!ok) throw new Win32Exception(Marshal.GetLastWin32Error(), "Journal integrity/account verification failed; preserve recovery evidence");
            byte[] result = new byte[output.Length]; Marshal.Copy(output.Data, result, 0, result.Length); return result;
        }
        finally { Marshal.FreeHGlobal(input.Data); Marshal.FreeHGlobal(entropy.Data); if (output.Data != IntPtr.Zero) LocalFree(output.Data); }
    }
}
