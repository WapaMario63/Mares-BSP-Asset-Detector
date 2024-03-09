using MareAssetDetector.Pack;
using MareAssetDetector.Utilities;

namespace MareAssetDetector
{
    public static class Keys
    {
        public static List<string> vmfSoundKeys = new List<string>();
        public static List<string> vmfModelKeys = new List<string>();
        public static List<string> vmfMaterialKeys = new List<string>();
        public static List<string> vmtTextureKeyWords = new List<string>();
        public static List<string> vmtMaterialKeyWords = new List<string>();
    }

    // CompilePal's Pack.cs rewritten to ONLY detect files and write to a text file.
    class AssetDetector
    {
        private string mGameFolder;
        private string mBspPath;
        private List<string> mSourceDirectories = new List<string>();
        private static string mOutputFile = "./detected_assets.txt";

        public static bool genParticleManifest = true;

        public static KeyValuePair<string, string> particleManifest = new KeyValuePair<string, string>();

        public AssetDetector(string gameFolder, string bspPath, string? output = null)
        {
            mGameFolder = gameFolder;
            mBspPath = bspPath;
            mOutputFile = Context.OutputFile;
        }

        public Result Run(CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogLine("\nDetecting Assets...");

                if (!File.Exists(mBspPath))
                {
                    throw new FileNotFoundException("BSP not found.", mBspPath);
                }

                Keys.vmtTextureKeyWords =
                    File.ReadAllLines(System.IO.Path.Combine(Context.KeysFolder, "texturekeys.txt")).ToList();
                Keys.vmtMaterialKeyWords =
                    File.ReadAllLines(System.IO.Path.Combine(Context.KeysFolder, "materialkeys.txt")).ToList();
                Keys.vmfSoundKeys = File.ReadAllLines(System.IO.Path.Combine(Context.KeysFolder, "vmfsoundkeys.txt")).ToList();
                Keys.vmfMaterialKeys = File.ReadAllLines(System.IO.Path.Combine(Context.KeysFolder, "vmfmaterialkeys.txt"))
                    .ToList();
                Keys.vmfModelKeys = File.ReadAllLines(System.IO.Path.Combine(Context.KeysFolder, "vmfmodelkeys.txt")).ToList();

                Logger.LogLine("Finding sources of game assets...");
                mSourceDirectories = GetSourceDirectories(mGameFolder);

                if (Context.Verbose)
                {
                    Logger.LogLine("Source Directories:");
                    foreach (var dir in mSourceDirectories)
                        Logger.LogLine(dir);
                }

                Logger.LogLine("Reading BSP...");

                BSP map;
                try
                {
                    map = new BSP(new FileInfo(mBspPath));
                }
                catch (CompressedBSPException)
                {
                    Logger.LogLineError("BSP is compressed and cannot detect assets this way. Please decompress first with bspzip.");
                    return Result.BSP_COMPRESSED;
                }

                AssetUtils.findBspUtilityFiles(map, mSourceDirectories, Context.RenameNav, genParticleManifest);

                // TODO: dryrun

                if (genParticleManifest)
                {
                    map.particleManifest = particleManifest;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return Result.PROCESS_ABORTED;
                }

                string unpackDir = System.IO.Path.GetTempPath() + Guid.NewGuid();

                // BSP unpacking is left to the main program
                //UnpackBSP(unpackDir);
                //AssetUtils.findBspPakDependencies(map, unpackDir);

                Logger.LogLine("Starting to write to file...");

                PackFile pakfile = new PackFile(map, mSourceDirectories, mOutputFile, Context.noSwVtx);

                // No VPK support, no idea what its for as of writing. This is meant for TF2 for now.

                if (cancellationToken.IsCancellationRequested)
                {
                    return Result.PROCESS_ABORTED;
                }

                Logger.LogLine("Writing File list...");

                pakfile.OutputToFile();

                // TODO: dryrun

                Logger.LogLine("Finished!");
                Logger.LogLine("---------------------------------");
                Logger.LogLine(pakfile.vmtcount + " materials found");
                Logger.LogLine(pakfile.mdlcount + " models found");
                Logger.LogLine(pakfile.pcfcount + " particle files found");
                Logger.LogLine(pakfile.soundcount + " sounds found");
                if (pakfile.vehiclescriptcount != 0)
                    Logger.LogLine(pakfile.vehiclescriptcount + " vehicle scripts found");
                if (pakfile.effectscriptcount != 0)
                    Logger.LogLine(pakfile.effectscriptcount + " effect scripts found");
                if (pakfile.vscriptcount != 0)
                    Logger.LogLine(pakfile.vscriptcount + " vscripts found");
                if (pakfile.PanoramaMapBackgroundCount != 0)
                    Logger.LogLine(pakfile.PanoramaMapBackgroundCount + " Panorama map backgrounds found");
                if (map.res.Count != 0)
                    Logger.LogLine(map.res.Count + " res files found");
                string additionalFiles =
                    (map.nav.Key != default(string) ? "\n-Nav file" : "") +
                    (map.soundscape.Key != default(string) ? "\n-Soundscape" : "") +
                    (map.soundscript.Key != default(string) ? "\n-Soundscript" : "") +
                    (map.detail.Key != default(string) ? "\n-Detail file" : "") +
                    (map.particleManifest.Key != default(string) ? "\n-Particle manifest" : "") +
                    (map.radartxt.Key != default(string) ? "\n-Radar files" : "") +
                    (map.RadarTablet.Key != default(string) ? "\n-Radar tablet" : "") +
                    (map.txt.Key != default(string) ? "\n-Loading screen text" : "") +
                    (map.jpg.Key != default(string) ? "\n-Loading screen image" : "") +
                    (map.PanoramaMapIcon.Key != default(string) ? "\n-Panorama map icon" : "") +
                    (map.kv.Key != default(string) ? "\n-KV file" : "");

                if (additionalFiles != "")
                    Logger.LogLine("Additional Files: " + additionalFiles);
                Logger.LogLine("---------------------------------");
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError($"Could not find {e.FileName}\n");
                return Result.GENERIC_FILE_NOT_FOUND;
            }
            catch (ThreadAbortException)
            {
                return Result.PROCESS_ABORTED;
                throw;
            }
            catch (Exception e)
            {
                Logger.LogLine("Something broke.");
                Logger.LogError($"{e}\n");
                return Result.UNKNOWN_ERROR;
            }

            return Result.SUCCESS;
        }

        public static List<string> GetSourceDirectories(string gamePath, bool verbose = true)
        {
            List<string> sourceDirectories = new List<string>();
            string gameInfoPath = System.IO.Path.Combine(gamePath, "gameinfo.txt");
            string rootPath = Directory.GetParent(gamePath).ToString();

            if (!File.Exists(gameInfoPath))
            {
                Logger.LogError($"Couldn't find gameinfo.txt at {gameInfoPath}");
                return new List<string>();
            }

            var gameInfo = new KV.FileData(gameInfoPath).headnode.GetFirstByName("GameInfo");
            if (gameInfo == null)
            {
                Logger.LogLineDebug($"Failed to parse GameInfo: {gameInfo}");
                Logger.LogError($"Failed to parse GameInfo, did not find GameInfo block\n");
                return new List<string>();
            }

            var searchPaths = gameInfo.GetFirstByName("FileSystem")?.GetFirstByName("SearchPaths");
            if (searchPaths == null)
            {
                Logger.LogLineDebug($"Failed to parse GameInfo: {gameInfo}");
                Logger.LogError($"Failed to parse GameInfo, did not find GameInfo block\n");
                return new List<string>();
            }

            foreach (string searchPath in searchPaths.values.Values)
            {
                // ignore unsearchable paths. TODO: will need to remove .vpk from this check if we add support for packing from assets within vpk files
                if (searchPath.Contains("|") && !searchPath.Contains("|gameinfo_path|") || searchPath.Contains(".vpk")) continue;

                // wildcard paths
                if (searchPath.Contains("*"))
                {
                    string fullPath = searchPath;
                    if (fullPath.Contains(("|gameinfo_path|")))
                    {
                        string newPath = searchPath.Replace("*", "").Replace("|gameinfo_path|", "");
                        fullPath = System.IO.Path.GetFullPath(gamePath + "/" + newPath.TrimEnd('/'));
                    }
                    if (Path.IsPathRooted(fullPath.Replace("*", "")))
                    {
                        fullPath = fullPath.Replace("*", "");
                    }
                    else
                    {
                        string newPath = fullPath.Replace("*", "");
                        fullPath = System.IO.Path.GetFullPath(rootPath + "/" + newPath.TrimEnd('/'));
                    }

                    if (verbose)
                        Logger.LogLine("Found wildcard path: {0}", fullPath);

                    try
                    {
                        var directories = Directory.GetDirectories(fullPath);
                        sourceDirectories.AddRange(directories);
                    }
                    catch { }
                }
                else if (searchPath.Contains("|gameinfo_path|"))
                {
                    string fullPath = gamePath;

                    if (verbose)
                        Logger.LogLine("Found search path: {0}", fullPath);

                    sourceDirectories.Add(fullPath);
                }
                else if (Directory.Exists(searchPath))
                {
                    if (verbose)
                        Logger.LogLine("Found search path: {0}", searchPath);

                    sourceDirectories.Add(searchPath);
                }
                else
                {
                    try
                    {
                        string fullPath = System.IO.Path.GetFullPath(rootPath + "/" + searchPath.TrimEnd('/'));

                        if (verbose)
                            Logger.LogLine("Found search path: {0}", fullPath);

                        sourceDirectories.Add(fullPath);
                    }
                    catch (Exception e)
                    {
                        Logger.LogDebug("Failed to find search path: " + e);
                        Logger.LogError($"Search path invalid: {rootPath + "/" + searchPath.TrimEnd('/')}");
                    }
                }
            }

            // Commented for now, maybe re-implement cross-game asset detection in the future?

            // find Chaos engine game mount paths
            // var mountedDirectories = GetMountedGamesSourceDirectories(gameInfo, Path.Combine(gamePath, "cfg", "mounts.kv"));
            // if (mountedDirectories != null)
            // {
            //     sourceDirectories.AddRange(mountedDirectories);
            //     foreach (var directory in mountedDirectories)
            //     {
            //         Logger.LogLine($"Found mounted search path: {directory}");
            //     }
            // }

            return sourceDirectories.Distinct().ToList();
        }

    }
}