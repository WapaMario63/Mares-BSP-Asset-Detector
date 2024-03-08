using System.Runtime.InteropServices;

namespace MareAssetDetector
{
    // Error Codes when the program exits.
    // This is necessary for the main Mare's BSP Compiler program to fetch status codes after it runs this program.
    public enum Result
    {
        SUCCESS = 0,
        UNKNOWN_ERROR = 1,
        INSUFFICIENT_ARGUMENTS = 2,
        GENERIC_FILE_NOT_FOUND = 3,
        BSP_COMPRESSED = 4,
        PROCESS_ABORTED = 5,
        // FILE_CORRUPT = 6,
        // PARTICLE_MANIFEST_NOT_FOUND = 7,
        // VSCRIPT_FILE_NOT_FOUND = 8,
        // PCF_MISSING = 9,
    }

    // Singleton storing map's own path, the game folder used to detect assets, optional output file and arguments.
    public static class Context
    {
        public static string BSPPath { get; set; } = "";
        public static string GameFolder { get; set; } = "";
        public static string OutputFiles { get; set; } = "./detected_assets.txt";
        public static bool Verbose = false;
        public static bool RenameNav = false;
        public static bool noSwVtx = false;

        // Some things (mainly the filesystem) are different on other platforms
        public static bool IsPlatformLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
        public static bool IsPlatformWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }


}