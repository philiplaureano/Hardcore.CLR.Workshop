using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    public abstract class BaseAssemblyVerificationTestFixture
    {
        private static readonly ConcurrentBag<string> DisposalList = new ConcurrentBag<string>();

        [SetUp]
        public void Init()
        {
            OnInit();
        }

        [TearDown]
        public void Term()
        {
            OnTerm();

            lock (DisposalList)
            {
                // Delete the files tagged for removal
                foreach (var file in DisposalList.Where(File.Exists))
                {
                    File.Delete(file);
                }
            }
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnTerm()
        {
        }

        protected static void AutoDelete(string filename)
        {
            if (DisposalList.Contains(filename) || !File.Exists(filename))
                return;

            DisposalList.Add(filename);
        }

        protected void PEVerify(string assemblyLocation)
        {
            var pathKeys = new[]
            {
                "sdkDir",
                "x86SdkDir"
            };

            var process = new Process();
            var peVerifyLocation = string.Empty;


            peVerifyLocation = GetVerifierLocation(pathKeys, peVerifyLocation);

            if (!File.Exists(peVerifyLocation))
            {
                Assert.Inconclusive("Warning: PEVerify.exe could not be found. Skipping test.");
            }

            process.StartInfo.FileName = peVerifyLocation;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            process.StartInfo.Arguments = "\"" + assemblyLocation + "\" /VERBOSE";
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            var processOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var result = string.Format("PEVerify Exit Code: {0}", process.ExitCode);

            Console.WriteLine(GetType().FullName + ": " + result);

            if (process.ExitCode == 0)
                return;

            Console.WriteLine(processOutput);
            Assert.Fail("PEVerify output: " + Environment.NewLine + processOutput, result);
        }

        private static string GetVerifierLocation(IEnumerable<string> pathKeys, string peVerifyLocation)
        {
            foreach (var key in pathKeys)
            {
                var directory = ConfigurationManager.AppSettings[key];

                if (string.IsNullOrEmpty(directory))
                    continue;

                peVerifyLocation = Path.Combine(directory, "peverify.exe");

                if (File.Exists(peVerifyLocation))
                    break;
            }
            return peVerifyLocation;
        }
    }
}
