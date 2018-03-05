using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public enum ExtractResult
    {
        SUCCESS = 0,
        INVALID_OPTIONS = 1,
        INPUT_ERROR = 2,
        OUTPUT_FILE_EXISTS = 3,
        ERROR_TRANSFORMING_DATA = 4
    }

    public enum ExitResult
    {
        SUCCESS = 0,
        INVALID_OPTIONS = 1,
        INPUT_FILE_ERROR = 2,
        OUTPUT_FILE_EXISTS = 3,
        ERROR_TRANSFORMING_DATA = 4,
        UNHANDLED_RUNTIME_EXCEPTION = 99
    }
}
