using System;
using System.Collections.Generic;
using System.Text;

using Python.Runtime;
using Xunit;

namespace ClassifyBot.Tests
{
    public class PythonScriptTests
    {
        public PythonScriptTests()
        {

        }

        /*
        [Fact(DisplayName = "Can initialize a Python script instance.")]
        public void CanInitPythonScript()
        {
            PythonScript m = new PythonScript(@"C:\Python\venv\TCC\Scripts");
            m.Init();
            Assert.True(m.Initialized);
            Assert.True(new PythonScript().Init());
            Assert.False(new PythonScript(@"C:\Python\venv\TCC\Scripts").Init());

        }

        [Fact(DisplayName = "Can get loaded Python modules.")]
        public void CanGetPythonModules()
        {
            PythonScript m = new PythonScript(@"C:\Python\venv\TCC\Scripts");
            m.Init();
            //m.GetModules();

        }
        */
    }
}
