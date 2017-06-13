using System;
using System.IO;
using ILRewriter;
using ILRewriter.Extensions;
using Mono.Cecil;
using NUnit.Framework;
using SampleLibrary;

namespace Tests
{
    [TestFixture]
    public class SampleTestFixture : BaseCecilTestFixture
    {

        [Test]
        public void Should_modify_console_writeline_string()
        {
            var modifiedAssembly = RewriteAssemblyOf<SampleClassWithInstanceMethod>();
            var modifiedType = CreateModifiedType(modifiedAssembly, nameof(SampleClassWithInstanceMethod));

            var myStream = new MemoryStream();
            modifiedAssembly.Write(myStream);

            var targetFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.dll");
            File.WriteAllBytes(targetFile, myStream.ToArray());

            PEVerify(targetFile);

            // Call the DoSomething() method
            // with the modified Console.WriteLine call
            modifiedType.DoSomething();
            return;
        }
        protected override IAssemblyModifier GetAssemblyModifier()
        {
            return new MethodCallInterceptionModifier();
        }
    }
}