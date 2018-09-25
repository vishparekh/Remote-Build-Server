/////////////////////////////////////////////////////////////////////
// ChildBuilder.cs -   This package provides core functionality    //
//                of building files for Build Server system        //
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
 * This Package procesess the build request send to it and builds the file. It communicates through WCF and receives files from Repository 
 * and sends the log file to repository.
 * 
 * Public Interface:
 * ================= 
 * void sendReady() : Sends ready message to Mother Builder
 * void buildCode(string testdriver, string sourecCode) : Builds the code
 * void createTemporaryDirectory() : Creates a temporary directory where all the files are stored
 * List<Tuple<string,string>> parseBuildRequest(string body) : Parses the build request
 * void initiateBuild(List<Tuple<string,string>> listTuple) : Initiate Build by getting files
 * void sendLogFile() : Send build log file to repository
 * void requestFiles(List<string>files) : Requests all the files require to build
 * void receiveMessages() : Receive Messages
 * string createTestRequest() : Creates a test request
 * void sendTestRequest(string testRequest) : Sends test request to test harness
 * 
 * 
 * Build Process:
 * --------------
 * Required Files: BuildRequest.cs Logger.cs MPCommService.cs IMPCommService.cs
   Build Command: csc BuildRequest.cs Logger.cs MPCommService.cs IMPCommService.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/

using CoreBuildServer;
using MessagePassingComm;
using PluggableRepoClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ChildBuilder
{
    public class ChildBuilder
    {
        Comm c;
        int port;
        Logger log;
        string storagepath;
        List<string> testRequestFiles;
        string baseAddress = "http://localhost";
        Random r;

        //creates a communication channel with the given port

        public ChildBuilder(int port)
        {
            c = new Comm(baseAddress, port);
            log = new Logger("..//..//..//BuildStorage" + port + "//BuildLog_" + port+".txt");
            storagepath = "..\\..\\..\\BuildStorage" + port.ToString();
            r = new Random();
            testRequestFiles = new List<string>();

        }

        //the child builder sends a ready message when it gets created or when it gets free and gets into the ready Queue

        void sendReady()
        {
            string from = baseAddress + ":" + port + "/IPluggableComm";
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.ready);
            csndMsg.command = "show";
            csndMsg.author = "Vishal Parekh";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = from;
            csndMsg.body = port.ToString();
            c.postMessage(csndMsg);
        }

        //builds the test driver and source code files

        void buildCode(string testdriver, string sourecCode)
        {
            string t = testdriver;
            string s = sourecCode;
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            myProcess.StartInfo.Arguments = "/Ccsc /define:DEBUG /warn:1 /target:library " + t + " " + s;
            myProcess.StartInfo.WorkingDirectory = @"../../../Repository";
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.Start();
            Console.WriteLine("\n\n Build Process Started");

            myProcess.WaitForExit();

            Console.Write(myProcess.ExitCode);  

            if (myProcess.ExitCode == 1)
            {
                log.ErrorLog("Error in Code\n");

            }
            else
            {
                log.ErrorLog("Build Succesfull");
                testRequestFiles.Add(testdriver);
            }
           
            string errors = myProcess.StandardError.ReadToEnd();
            string output = myProcess.StandardOutput.ReadToEnd();
            log.ErrorLog(errors);
            log.ErrorLog(output);
            Console.WriteLine("\n\n Build Process Completed");
            Console.WriteLine(errors);
            Console.WriteLine(output);
        }

        //creates a temporary dircetory to plce files from repository

        void createTemporaryDirectory()
        {
            System.IO.Directory.CreateDirectory(Path.GetFullPath(storagepath));
        }

        //parses the build request and sends the result to build

        List<Tuple<string,string>> parseBuildRequest(string body)
        {

            XDocument doc = XDocument.Parse(body);
            IEnumerable<XElement> e = doc.Root.Descendants("test");

            List<Tuple<string, string>> listTuple = new List<Tuple<string, string>>();
            List<string> files = new List<string>();
            foreach (XElement x in e)
            {
                string testDriver = x.Descendants("testDriver").First().Value;
                Console.WriteLine("transferring file \"{0}\"", testDriver);
                files.Add(testDriver);

                IEnumerable<XElement> tested = x.Descendants("tested");
                string sourceCode = " ";
                foreach (XElement elem in tested)
                {
                    Console.WriteLine(elem.ToString());
                    Console.WriteLine("transferring file \"{0}\"", elem.Value);
                    files.Add(elem.Value);
                    sourceCode += elem.Value + " ";
                }

                Tuple<string, string> t = new Tuple<string, string>(testDriver, sourceCode);
                listTuple.Add(t);
                requestFiles(files);
            }
            return listTuple;
        }

        //it initiates the build process

        void initiateBuild(List<Tuple<string,string>> listTuple)
        {
            foreach (var obj in listTuple)
            {
                Console.WriteLine(obj.Item1 + " " + obj.Item2);
                buildCode(obj.Item1, obj.Item2);
            }
        }

        //sends the log file to repository

        void sendLogFile()
        {
            string fromStorage1 = storagepath;
            string toStorage1 = "..//..//..//Repository";
            c.postFile("BuildLog_" + port + ".txt", fromStorage1, toStorage1);
        }

        //requests the required file from the repository and places in the temporary directory of Build Server

        void requestFiles(List<string>files)
        {
            CommMessage requestFileTransfer = new CommMessage(CommMessage.MessageType.fileTransfer);
            requestFileTransfer.command = "show";
            requestFileTransfer.author = "Jim Fawcett";
            requestFileTransfer.body = storagepath;
            requestFileTransfer.files = files;
            requestFileTransfer.to = "http://localhost:8081/IPluggableComm";
            requestFileTransfer.from = "http://localhost:" + port + "/IPluggableComm";
            c.postMessage(requestFileTransfer);
        }

        //starts listening for messages

        void receiveMessages()
        {
            while (true)
            {
                CommMessage cd = c.getMessage();
                if (cd.type.ToString() == "buildRequest")
                {
                    Console.WriteLine("\n\n Build Request Received");

                    createTemporaryDirectory();

                    string toStorage = storagepath;

                    Console.WriteLine("\n Child Builder Started");

                    List<Tuple<string, string>> tupleList = parseBuildRequest(cd.body);

                    initiateBuild(tupleList);

                    sendLogFile();

                    string testRequest = createTestRequest();

                    sendTestRequest(testRequest);

                    sendReady();

                    Console.WriteLine("\n\n Build Request Processed");

                }
                if (cd.type.ToString() == "close")
                {
                    Console.WriteLine("Process getting terminated");
                }
                cd.show();
            }
        }

        //creates a test request

        string createTestRequest()
        {

            XmlDocument xmlDoc = new XmlDocument();

            XmlNode testRequestElem = xmlDoc.CreateElement("testRequest");
            xmlDoc.AppendChild(testRequestElem);

            XmlNode author = xmlDoc.CreateElement("author");
            author.InnerText = "Vishal Parekh";
            testRequestElem.AppendChild(author);

            XmlNode datetime = xmlDoc.CreateElement("datetime");
            datetime.InnerText = (DateTime.Now.ToString());
            testRequestElem.AppendChild(datetime);

            foreach(string t in testRequestFiles)
            {
                XmlNode rootNode = xmlDoc.CreateElement("tests");
                XmlNode userNode = xmlDoc.CreateElement("testLibrary");
                int temp = t.IndexOf(".");
                string temp1 = t.Substring(0, temp);
                temp1 = temp1 + ".dll";
                userNode.InnerText = temp1;
                rootNode.AppendChild(userNode);
                testRequestElem.AppendChild(rootNode);
            }
            string fileName = "..//..//..//Repository/TestRequest_" + r.Next(1, 100) + ".xml";
            xmlDoc.Save(fileName);
            string testRequest = File.ReadAllText(fileName);
            return testRequest;
        }

        //sends the test reuqest to test harness

        void sendTestRequest(string testRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.testRequest);
            csndMsg.command = "show";
            csndMsg.author = "Vishal Parekh";
            csndMsg.to = "http://localhost:8095/IPluggableComm";
            csndMsg.from = "http://localhost:"+port+"/IPluggableComm";
            csndMsg.body = testRequest;
            c.postMessage(csndMsg);
            Console.Write("\n   Sending message from {0} to {1}", csndMsg.from, csndMsg.to);
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Child Builder - Listening at port:" + Int32.Parse(args[0]));
                Console.WriteLine("\n======================================");
                
                ChildBuilder cb = new ChildBuilder(Int32.Parse(args[0]));
                cb.port = Int32.Parse(args[0]);
                cb.sendReady();
                cb.receiveMessages();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }
}
