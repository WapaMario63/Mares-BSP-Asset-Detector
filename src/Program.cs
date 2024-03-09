using MareAssetDetector;
using MareAssetDetector.Pack;
using MareAssetDetector.Utilities;

namespace MareAssetDetector
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Logger.LogLineError("No map and game folder passed.");
                Logger.LogLine("Usage: assetdetector <map bsp> <game folder>" +
                                    "\n\t-o <output file path> \t-Output bspzip ready packing text in a different file than the default 'detected_assets.txt' at the root of this program." +
                                    "\n\t-verbose\t- Print more information." +
                                    "\n\t-renamenav"+
                                    "\n\t-no-swvtx\t- Do not include .sw.vtx files in the packing file." +
                                    "\n\t-keys-dir <keys directory>\t - Use a different folder to search for material keys.");
                return (int)Result.INSUFFICIENT_ARGUMENTS;
            }

            Context.BSPPath = args[0];
            Context.GameFolder = args[1];

            Logger.LogLine($"Looking for assets in: {Context.BSPPath}");
            Logger.LogLine($"Using game folder: {Context.GameFolder}");

            for (int i=2; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "-o":
                        Context.OutputFile = args[i+1];
                        break;
                    case "-verbose":
                        Context.Verbose = true;
                        break;
                    case "-renamenav":
                        Context.RenameNav = true;
                        break;
                    case "-no-swvtx":
                        Context.noSwVtx = true;
                        break;
                    case "-keys-dir":
                        Context.KeysFolder = args[i+1];
                        break;
                }
            }

            return Detect(Context.BSPPath, Context.GameFolder);
        }

        public static int Detect(string bspPath, string gameFolder)
        {
            AssetDetector detector = new AssetDetector(gameFolder, bspPath);
            CancellationTokenSource cancelSource = new CancellationTokenSource();

            Result result = detector.Run(cancelSource.Token);

            return (int)result;
        }


    }
}