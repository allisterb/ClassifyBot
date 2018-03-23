using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace ClassifyBot.Tests
{
    public class PythonModuleTests
    {
        public PythonModuleTests()
        {

        }

        [Fact]
        public void CanInitPythonModule()
        {
            PythonScript m = new PythonScript(@"C:\Python\venv\TCC\Scripts", @"C:\Projects\ClassifyBot\TCCC\classifier.py");
            m.Init();
        }
    }
}
