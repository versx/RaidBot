namespace T.Ocr
{
    using System;
    using System.Threading.Tasks;

    public interface IOcrService
    {
        bool SaveDebugImages { get; }

        Task<RaidOcrResult> AddRaidAsync(string filePath, bool testMode);
    }
}