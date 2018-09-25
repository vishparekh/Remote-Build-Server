using System;
using System.Collections.Generic;
using System.Text;

namespace TestHarness
{
    interface ITest
    {
        bool test();
    }
    public class TestDriver1 : ITest
    {   
        public bool test()
        {
             CodeToTest1 code = new CodeToTest1();
             if (code.add(1, 2) == 5 && code.minus(2,1) ==  1)
                 return true;
             else 
                return false;
        }
        static void Main(string[] args)
        {
              Console.Write("\n  TestDriver running:");
              TestDriver1 td = new TestDriver1();
              td.test();
              Console.Write("\n\n");
        }
    }
}