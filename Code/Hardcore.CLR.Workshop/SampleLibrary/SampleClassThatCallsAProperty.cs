using System;

namespace SampleLibrary
{
    public class SampleClassThatCallsAProperty
    {
        public void DoSomething()
        {
            var instance = new SampleClassWithProperties(42);
            Console.WriteLine("The value is: {0}", instance.Value);
        }
    }
}