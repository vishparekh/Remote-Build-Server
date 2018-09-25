/////////////////////////////////////////////////////////////////////
// DLLLoaderExec.cs : Loads the dll file and runs it. The simulated//
//test function helps to get know the test has passed or failed.   //
// version: 1.0                                                    //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Dell Inspiron 15, Windows 10                       //
// Application: Remote Build Server Prototypes                     //
//                                                                 //
// Source: Dr. Jim Fawcett                                         //
// Author Name : Vishal Parekh                                     //
// CSE681: Software Modeling and Analysis, Fall 2017               //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package demonstartes the loading and executing of dynamic link library file.
 * 
 * Public Interface:
 * ================= 
 * static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args) : Loads the test libraries
 * public string loadAndExerciseTesters(string dllFile) : Runs the test libraries (.dll files)
 * bool runSimulatedTest(Type t, Assembly asm) : Runs the simulated test
 * public string GuessTestersParentDir() : Sets the directory
 * 
 * Build Process:
 * --------------
 * Required Files: Logger.cs MPCommService.cs IMPCommService.cs TestHarness.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuildServer
{
    public interface ITest      // interface for test driver
    {
        bool test();
    }
    class DllLoaderExec
    {
        Logger log = new Logger("..//..//..//TestStorage//TestLog_8095.txt");

        public static string testersLocation { get; set; } = ".";

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  Called binding error event handler");
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        //----< load assemblies from testersLocation and run their tests >-----

        public string loadAndExerciseTesters(string dllFile)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);

            try
            {
                DllLoaderExec loader = new DllLoaderExec();

                // load each assembly found in testersLocation

                string[] files = Directory.GetFiles(testersLocation, "*.dll");
                
                Assembly asm = Assembly.Load(File.ReadAllBytes(testersLocation + dllFile));
                // exercise each tester found in assembly

                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    // if type supports ITest interface then run test

                    if (t.GetInterface("TestHarness.ITest", true) != null)
                        if (!loader.runSimulatedTest(t, asm))
                            Console.Write("\n  Test {0} failed to run", t.ToString());
                }
                //}
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Testing completed";
        }
        //
        //----< run tester t from assembly asm >-------------------------------

        bool runSimulatedTest(Type t, Assembly asm)
        {
            try
            {
                Console.Write(
                  "\n  Attempting to create instance of {0}", t.ToString()
                  );
                object obj = asm.CreateInstance(t.ToString());
                MethodInfo method = t.GetMethod("add");
                if (method != null)
                    method.Invoke(obj, new object[0]);
                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);
                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "Passed";
                    return "Failed";
                };
                log.ErrorLog("Test :"+act(status));
                Console.Write("\n  Test {0}", act(status));
            }
            catch (Exception ex)
            {
                Console.Write("\n  Test failed with message \"{0}\"", ex.Message);
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            //  You would think that the code below should work, but it fails
            //  with invalidcast exception, even though the types are correct.
            //
            //    DllLoaderDemo.ITest tester = (DllLoaderDemo.ITest)obj;
            //    tester.say();
            //    tester.test();
            //
            //  This is a design feature of the .Net loader.  If code is loaded 
            //  from two different sources, then it is considered incompatible
            //  and typecasts fail, even thought types are Liskov substitutable.
            //
            return true;
        }
        //
        //----< extract name of current directory without its parents ---------

        public string GuessTestersParentDir()
        {
            string dir = Directory.GetCurrentDirectory();
            int pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
            string name = dir.Remove(0, pos + 1).ToLower();
            if (name == "debug")
                return "../..";
            else
                return "../..";
        }   
        //----< run demonstration >--------------------------------------------
    }

#if (TEST_DLLLOADEREXEC)
    class DllLoaderExecTest
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrating Robust Test Loader");
            Console.Write("\n ==================================\n");

            DllLoaderExec loader = new DllLoaderExec();

            if (args.Length > 0)
                DllLoaderExec.testersLocation = args[0];
            else
                DllLoaderExec.testersLocation = loader.GuessTestersParentDir() + "/Testers";

            // convert testers relative path to absolute path

            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", DllLoaderExec.testersLocation);

            // run load and tests

            string result = loader.loadAndExerciseTesters();

            Console.Write("\n\n  {0}", result);
            Console.Write("\n\n");
        }
    }
#endif
}
