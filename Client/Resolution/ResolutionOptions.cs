namespace Client.Resolution
{
    internal static class ResolutionOptions
    {
        public static Client.Bootstrap.ResolutionOptions.Option[] All => Client.Bootstrap.ResolutionOptions.All;
        public static int IndexOf(int w, int h) => Client.Bootstrap.ResolutionOptions.IndexOf(w, h);
        public static Client.Bootstrap.ResolutionOptions.Option Resolve(int w, int h, int max) => Client.Bootstrap.ResolutionOptions.Resolve(w, h, max);
    }
}
