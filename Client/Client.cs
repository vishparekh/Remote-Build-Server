/////////////////////////////////////////////////////////////////////
// Builder.cs -   This package retrieves Build Request and         //
//                processes further                                //
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
 * This package receives file from the repository. Client communicates with other component through WCF 
 * 
 * Public Interface:
 * ================= 
 * void processBuildRequest() : processes the build request and sends message to mother builder
 * void sendBuildRequest(string testRequest) : Sends the build request and it gets added in the queue
 * void receiveMessage() : receives message from other component through WCF
 * 
 * Build Process:
 * --------------
 * Required Files: BuildRequest.cs Logger.cs IMPCommService.cs MPCommService.cs BlockingQueue.cs
   Build Command: csc BuildRequest.cs Logger.cs IMPCommService.cs MPCommService.cs BlockingQueue.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/


using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3Help
{
    public class Client
    {
        Comm c;
        string fromStorage = "..//..//..//Repository";

        public Client()
        {
            c = new Comm("http://localhost", 8081);
        }

        //processes repository and selects the build request files

        void processBuildRequest()
        {
            string[] fileEntries = Directory.GetFiles(Directory.GetCurrentDirectory() + "..//..//..//..//Repository");

            foreach (string file in fileEntries)
            {
                if (file.Contains("BuildRequest"))
                {
                    string path = Directory.GetCurrentDirectory() + "..//..//..//..//Repository//" + Path.GetFileName(file);
                    string testRequest = File.ReadAllText(path);
                    sendBuildRequest(testRequest);
                }
            }
        }

        //sends a message to childbuilder with build request in body part of message

        void sendBuildRequest(string testRequest)
        {

            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "show";
            csndMsg.author = "Vishal Parekh";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = "http://localhost:8081/IPluggableComm";
            csndMsg.body = testRequest;
            c.postMessage(csndMsg);
            Console.Write("\n   Sending message from {0} to {1}", csndMsg.from, csndMsg.to);

        }

        //receives messages from other component through WCF

        void receiveMessage()
        {
            while (true)
            {
                CommMessage c1 = c.rcvr.getMessage();
                string toStorage1 = c1.body;
                Console.Write(c1.body);

                if (c1.type.ToString() == "fileTransfer")
                {
                    Console.WriteLine("\n\n File Transfer request received");

                    List<string> files = c1.files;
                    foreach (string file in files)
                    {
                        Console.WriteLine("\n transferring file \"{0}\"", file);

                        bool transferFile = c.postFile(file, fromStorage, toStorage1);
                        if (transferFile)
                            Console.WriteLine("\n Successfully file transferred!");
                    }
                    Console.WriteLine("\n\n File Transfer completed");

                }
                c1.show();
            }
        }

        static void Main(string[] args)
        {

            Console.WriteLine("Client - Listening at port:8081");
            Console.WriteLine("\n======================================");

            try
            {
                Client client = new Client();
                client.processBuildRequest();
                client.receiveMessage();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}