using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AnySerializer.Tests
{
    public static class TestHelper
    {
        public static string GetResourceFileText(string filename)
        {
            var result = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream =
                assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}"))
            {
                using (var sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
