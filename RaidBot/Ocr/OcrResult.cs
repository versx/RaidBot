namespace T.Ocr
{
    using System;
    using System.Collections.Generic;

    public class OcrResult<T>
    {
        public bool IsSuccess { get; }

        public KeyValuePair<T, double>[] Results { get; }

        public string OcrValue { get; }

        public OcrResult(bool isSuccess, string ocrValue, KeyValuePair<T, double>[] results = null)
        {
            IsSuccess = isSuccess;
            OcrValue = ocrValue;
            Results = results;
        }

        public T GetFirst()
        {
            return Results[0].Key;
        }
    }
}