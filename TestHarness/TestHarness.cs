/////////////////////////////////////////////////////////////////////
// CTestHarness.cs : Attempts to load and execute each test library// 
//,catching any execeptions that may be emitted, and report sucess // 
//or failure and any exception messages, to the Console.           //
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
 * This package demonstartes the loading and executing of dynamic link library file from TestStorage. 
 * It also dislays the test result on the console.
 * 
 * Public Interface:
 * ================= 
 * void sendLogFile() : Sends log file to repository
 * List<string> parseTestRequest(string body) : Parses the test request
 * public void requestFiles(List<string> files) : Request files from repository and stores in Test Storage
 * public void initiateLoadAndTest(List<string> files) : Loads and runs the test library
 * void receiveMessages() : Receive messages
 * 
 * Build Process:
 * --------------
 * Required Files: BuildRequest.cs Logger.cs MPCommService.cs IMPCommService.cs ChildBuilder.cs DLLLoaderExec.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using CoreBuildServer;
using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace CoreBuildServer
{
    public class TestHarness
    {
        Comm c;
        Logger log;
        public TestHarness()
        {
            c = new Comm("http://localhost", 8095);
            log = new Logger("..//..//..//TestStorage//TestLog_8095.txt");
        }

        //Listens on the port to receive messages

        public void receiveMessages()
        {
            while (true)
            {
                CommMessage cd = c.getMessage();
                if (cd.type.ToString() == "testRequest")
                {
                    Console.WriteLine("\n\n Test Request Received");

                    Thread.Sleep(2000);

                    List<string> files = parseTestRequest(cd.body);
                    requestFiles(files);

                    Thread.Sleep(2000);
                    initiateLoadAndTest(files);

                    sendLogFile();

                    Console.WriteLine("\n\n Test Request Processed");

                }
                if (cd.type.ToString() == "close")
                {
                    Console.WriteLine("Process getting terminated");
                }
                cd.show();
            }
        }

        //sends log file to repository

        void sendLogFile()
        {
            string fromStorage1 = "..\\..\\..\\TestStorage";
            string toStorage1 = "..//..//..//Repository";
            c.postFile("TestLog_" + 8095 + ".txt", fromStorage1, toStorage1);
        }

        //parses the build request and sends the result to build

        List<string> parseTestRequest(string body)
        {
            Console.WriteLine("Body:: " + body);
            XDocument doc = XDocument.Parse(body);
            IEnumerable<XElement> e = doc.Root.Descendants("tests");
            List<string> files = new List<string>();
            foreach (XElement x in e)
            {
                string testDriver = x.Descendants("testLibrary").First().Value;
                int i = testDriver.IndexOf('.');
                testDriver = testDriver.Substring(0, i);
                testDriver = testDriver + ".dll";
                Console.WriteLine("transferring file \"{0}\"", testDriver);
                files.Add(testDriver);
                Console.Write("TestDriver::"+testDriver);
            }
            return files;
        }

        //request files from repository 

        public void requestFiles(List<string> files)
        {
            CommMessage requestFileTransfer = new CommMessage(CommMessage.MessageType.fileTransfer);
            requestFileTransfer.command = "show";
            requestFileTransfer.author = "Jim Fawcett";
            requestFileTransfer.body = "..\\..\\..\\TestStorage";
            requestFileTransfer.files = files;
            requestFileTransfer.to = "http://localhost:8081/IPluggableComm";
            requestFileTransfer.from = "http://localhost:" + "8095" + "/IPluggableComm";
            c.postMessage(requestFileTransfer);
        }

        //loads and runs the test libraries

        public void initiateLoadAndTest(List<string> files)
        {
            DllLoaderExec loader = new DllLoaderExec();

            DllLoaderExec.testersLocation = loader.GuessTestersParentDir() + "/../TestStorage/";

            // convert testers relative path to absolute path

            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", DllLoaderExec.testersLocation);
            foreach (var testDriver in files)
            {
                Console.Write(testDriver);
                string result = loader.loadAndExerciseTesters(testDriver);
                log.ErrorLog(result);

                Console.Write("\n\n  {0}", result);
                Console.Write("\n\n");

            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Test Harness - Listening at port: 8095");
            Console.WriteLine("\n======================================");
            TestHarness testHarnessObj = new TestHarness();
            testHarnessObj.receiveMessages();
        }
    }
}
