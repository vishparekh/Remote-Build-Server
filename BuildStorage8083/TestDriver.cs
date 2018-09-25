using System;
using System.Collections.Generic;
using System.Text;

namespace TestHarness
{
    interface ITest
    {
        bool test();
    }
    public class TestDriver : ITest
    {   
        public bool test()
        {
             CodeToTest code = new CodeToTest();
             if (code.add(1, 2) == 3 && code.minus(2,1) ==  1)
                 return true;
             else 
                return false;
        }
        static void Main(string[] args)
        {
              Console.Write("\n  TestDriver running:");
              TestDriver td = new TestDriver();
              td.test();
              Console.Write("\n\n");
        }
    }
}