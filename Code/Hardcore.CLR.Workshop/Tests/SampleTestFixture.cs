using ILRewriter;
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

            
            // Call the DoSomething() method
            // with the modified Console.WriteLine call
            modifiedType.DoSomething();
            return;
        }
        protected override IAssemblyModifier GetAssemblyModifier()
        {
            return new SampleAssemblyModifier();
        }
    }
}