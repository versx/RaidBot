namespace T.Ocr
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class OcrService : IOcrService, IDisposable
    {
        private RaidImageConfiguration _imageConfiguration;

        #region Properties

        public bool SaveDebugImages { get; set; }

        public string TesseractPath
        {
            get
            {
                var tesseractPath = string.Empty;
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                if (isWindows)
                {
                    // Default Windows installation
                    //tesseractPath = @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe";
                    tesseractPath = Path.Combine(RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86", "tesseract.exe");
                }
                else
                {
                    // Default Homebrew installation
                    tesseractPath = Path.Combine("/usr/local/Cellar/tesseract/3.05.01/bin", "tesseract");
                }
                return tesseractPath;
            }
        }

        public string TessdataPath => Path.Combine(".", "tessdata");

        public string OcrLanguages => string.Join("+", new string[] { "eng", "deu" });

        #endregion

        #region Public Methods

        public async Task<RaidOcrResult> AddRaidAsync(string filePath, bool testMode)
        {
            SaveDebugImages = testMode;
            using (var image = Image.Load(filePath))
            {
                _imageConfiguration = image.GetConfiguration(SaveDebugImages);
                _imageConfiguration.PreProcessImage(image);
                if (SaveDebugImages)
                {
                    image.Save("_AfterPreprocess.png");
                }

                var raidOcrResult = await GetRaidOcrResultAsync(image);
                if (!raidOcrResult.IsRaidImage)
                {
                    return null;
                }

                return raidOcrResult;
            }
        }

        public void Dispose()
        {
            //Context?.Dispose();
        }

        #endregion

        #region Implementation

        private async Task<RaidOcrResult> GetRaidOcrResultAsync(Image<Rgba32> image)
        {
            var result = new RaidOcrResult();
            var fragmentTypes = Enum.GetValues(typeof(RaidImageFragmentType)).Cast<RaidImageFragmentType>();

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            Parallel.ForEach(fragmentTypes, async type =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                using (var imageFragment = image.Clone(e => e.Crop(_imageConfiguration[type])))
                {
                    switch (type)
                    {
                        case RaidImageFragmentType.EggTimer:
                            result.EggTimer = await GetTimerValue(imageFragment, type);
                            break;
                        case RaidImageFragmentType.EggLevel:
                            result.EggLevel = await GetEggLevel(imageFragment);
                            break;
                        case RaidImageFragmentType.GymName:
                            result.Gym = await GetGym(imageFragment);
                            result.Gym = RemoveUnwantedCharacters(result.Gym);
                            if (!string.IsNullOrEmpty(result.Gym))
                            {
                                result.Gym = result.Gym.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            }
                            break;
                        case RaidImageFragmentType.PokemonName:
                            result.Pokemon = await GetPokemon(imageFragment);
                            break;
                        case RaidImageFragmentType.PokemonCp:
                            result.PokemonCp = await GetPokemonCp(imageFragment);
                            break;
                        case RaidImageFragmentType.RaidTimer:
                            result.RaidTimer = await GetTimerValue(imageFragment, type);
                            break;
                    }
                }
            });

            return await Task.FromResult(result);
        }

        private async Task<int> GetEggLevel(Image<Rgba32> imageFragment)
        {
            if (SaveDebugImages)
            {
                imageFragment.Save($"_{RaidImageFragmentType.EggLevel}_Step1_Analyze.png");
            }

            var whiteThreshold = 240;
            // Check the locations for level 1, 3 and 5 raids
            var whitePixelCount = _imageConfiguration.Level5Points.Select(levelPoint => imageFragment[levelPoint.X, levelPoint.Y]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);

            // No white pixels found so lets check the locations for level 2 and 4 raids
            if (whitePixelCount == 0)
            {
                whitePixelCount = _imageConfiguration.Level4Points.Select(levelPoint => imageFragment[levelPoint.X, levelPoint.Y]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);
            }

            // Make sure the level is within the possible range
            if (whitePixelCount < 1 || whitePixelCount > 5)
            {
                return await Task.FromResult(0);
            }

            return await Task.FromResult(whitePixelCount);
        }

        private async Task<string> GetGym(Image<Rgba32> imageFragment)
        {
            imageFragment = _imageConfiguration.PreProcessGymNameFragment(imageFragment);

            var ocrResult = await GetOcrResultAsync(imageFragment);
            return ocrResult;
        }

        private async Task<string> GetPokemon(Image<Rgba32> imageFragment)
        {
            imageFragment = _imageConfiguration.PreProcessPokemonNameFragment(imageFragment);

            var ocrResult = await GetOcrResultAsync(imageFragment);
            return ocrResult;
        }

        private async Task<int> GetPokemonCp(Image<Rgba32> image)
        {
            var imageFragment = _imageConfiguration.PreProcessPokemonCpFragment(image);
            var ocrResult = await GetOcrResultAsync(imageFragment);
            if (!(ocrResult.Length > 0)) return 0;

            var cpString = ocrResult.ToLowerInvariant();
            if (cpString.StartsWith("cp", StringComparison.OrdinalIgnoreCase) ||
                cpString.StartsWith("03", StringComparison.OrdinalIgnoreCase) ||
                cpString.StartsWith("c3", StringComparison.OrdinalIgnoreCase) ||
                cpString.StartsWith("0p", StringComparison.OrdinalIgnoreCase))
            {
                cpString = ocrResult.Substring(2).ToLowerInvariant();
            }

            var cp = GetDigitsOnly(cpString);
            cp = cp.Substring(Math.Max(cp.Length - 5, 0));
            if (!int.TryParse(cp, out int result))
            {
                //Failed
                return 0;
            }

            return result;
        }

        private async Task<TimeSpan> GetTimerValue(Image<Rgba32> imageFragment, RaidImageFragmentType imageFragmentType)
        {
            imageFragment = _imageConfiguration.PreProcessTimerFragment(imageFragment, imageFragmentType);
            var result = await GetOcrResultAsync(imageFragment);

            if (!string.IsNullOrEmpty(result) && TimeSpan.TryParse(result, out TimeSpan timeSpan))
            {
                return timeSpan;
            }

            return Timeout.InfiniteTimeSpan;
        }

        private async Task<string> GetOcrResultAsync(Image<Rgba32> imageFragment)
        {
            string output;
            var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();
            var tempImageFile = imageFragment.CreateTempImageFile();
            try
            {
                var args = new StringBuilder();
                args.Append("--tessdata-dir " + TessdataPath);
                args.Append(" " + tempImageFile);  // Image file.
                args.Append(" " + tempOutputFile); // Output file (tesseract add '.txt' at the end)
                args.Append(" -l " + OcrLanguages);    // Languages.
                args.Append(" " + Path.Combine(TessdataPath, "configs", "bazaar"));    // Config.

                var info = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    FileName = TesseractPath,
                    Arguments = args.ToString()
                };

                // Start tesseract.
                var process = Process.Start(info);
                if (process == null)
                {
                    throw new Exception("Unable to start the OCR-Recognition service.");
                }
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // Exit code: success.
                    output = File.ReadAllText(tempOutputFile + ".txt");
                }
                else
                {
                    throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
                }
            }
            finally
            {
                File.Delete(tempImageFile);
                File.Delete(tempOutputFile + ".txt");
            }

            var value = RemoveUnwantedCharacters(output);
            return await Task.FromResult(value);
        }

        private static string RemoveUnwantedCharacters(string input)
        {
            //return input;
            input = input.Replace("—", "-");
            var arr = input.ToCharArray();

            arr = Array.FindAll(arr, (c => (char.IsLetterOrDigit(c)
                                         || char.IsWhiteSpace(c)
                                         || c == '('
                                         || c == ')'
                                         //|| c == '.'
                                         || c == '\''
                                         || c == '-'
                                         || c == ':')));
            return new string(arr).TrimEnd('\n').Trim();
        }

        private static string GetDigitsOnly(string input)
        {
            var chars = input.ToCharArray();
            chars = Array.FindAll(chars, char.IsDigit);
            return new string(chars);
        }

        #endregion
    }
}