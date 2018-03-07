using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public enum StageResult
    {
        SUCCESS = 0,
        INVALID_OPTIONS = 1,
        INPUT_ERROR = 2,
        OUTPUT_ERROR = 3,
        FAILED = 4,
        CREATED = -100,
        INITIALIZED = -101
    }

    public enum ExtractResult
    {
        SUCCESS = 0,
        INVALID_OPTIONS = 1,
        INPUT_ERROR = 2,
        OUTPUT_FILE_EXISTS = 3,
    }

    public enum ExitResult
    {
        SUCCESS = 0,
        INVALID_OPTIONS = 1,
        INPUT_FILE_ERROR = 2,
        OUTPUT_FILE_EXISTS = 3,
        STAGE_FAILED = 90,
        UNHANDLED_RUNTIME_EXCEPTION = 99
    }
}
