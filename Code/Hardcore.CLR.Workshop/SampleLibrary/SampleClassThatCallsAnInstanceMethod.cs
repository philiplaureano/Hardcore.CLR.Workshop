using System;

namespace SampleLibrary
{
    public class SampleClassThatCallsAnInstanceMethod
    {
        public void DoSomething()
        {
            var self = new SampleClassThatCallsAnInstanceMethod();
            self.DoSomethingElse();
        }

        public void DoSomethingElse()
        {
            Console.WriteLine("DoSomethingElse called");
        }
    }
}