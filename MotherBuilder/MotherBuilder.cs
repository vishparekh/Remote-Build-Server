/////////////////////////////////////////////////////////////////////
// Builder.cs -   This package provides  functionality of managing //
//                process pool and assigns the build requests to   //
//                the child which are present in ready Queue       //
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
 * This Package receives build requests from the client which it places in the process pool 
 * and from here the mother builder will search for an available child builder in ready queue 
 * then it will assign the build request.
 * 
 * Public Interface:
 * ================= 
 *   bool KillProcessByID(int id, bool waitForExit = false) : kills the process according to their process ID
 *   void processQueue() : processes the process pool, if build request is their it will search for a child builder in ready queue and assign it
 *   void receiveMessages() : receives messages
 *   int selectChildPort() : selects the child builder from the ready queue
 *   static bool createProcess(int portNumber) : generates a child process according to the port number
 * 
 * Build Process:
 * --------------
 * Required Files: MotherBuilder.cs Logger.cs IMPCommService.cs MPCommServie
   Build Command: csc MotherBuilder.cs Logger.cs IMPCommService.cs MPCommServie

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/



using MessagePassingComm;
using SWTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project3Help
{
    public class MotherBuilder
    {
        Comm c = new Comm("http://localhost", 8080);
        BlockingQueue<int> ready;
        BlockingQueue<string> buildQ;
        static List<int> processID = new List<int>();
        static List<int> ports = new List<int>();

        public MotherBuilder()
        {
            ready = new BlockingQueue<int>();
            buildQ = new BlockingQueue<string>();
        }

        //kills the process according to process ID specfied

        bool KillProcessByID(int id, bool waitForExit = false)
        {
            using (Process p = Process.GetProcessById(id))
            {
                if (p == null || p.HasExited)
                    return false;
                p.Kill();
                if (waitForExit)
                {
                    p.WaitForExit();
                }
                return true;
            }
        }

        //reads the process pool if any build request found it will assign to child builder

        void processQueue()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    CommMessage csndMsg3 = new CommMessage(CommMessage.MessageType.buildRequest);
                    csndMsg3.command = "show";
                    csndMsg3.author = "Vishal Parekh";
                    csndMsg3.from = "http://localhost:" + "8080" + "/IPluggableComm";
                    csndMsg3.body = buildQ.deQ();
                    int port = ready.deQ();
                    csndMsg3.to = "http://localhost:" + port + "/IPluggableComm";
                    c.postMessage(csndMsg3);
                }
            });
            t.Start();
        }

        //received the messages sent through WCF from different component

        void receiveMessages()
        {
            while (true)
            {
                CommMessage c1 = c.rcvr.getMessage();

                if (c1.from == "http://localhost:8081/IPluggableComm")
                {
                    buildQ.enQ(c1.body);
                    Console.WriteLine("\n Build Request enqueued in the queue");
                }
                if (c1.type.ToString() == "ready")
                {
                    ready.enQ(Int32.Parse(c1.body));
                    Console.WriteLine("\n Child Builder is ready");
                }
                if (c1.type.ToString() == "close")
                {
                    Console.WriteLine("\n\n Close command received");
                    Console.WriteLine("\n\n Mother Builder and Child Builder gets closed");

                    foreach (int port in ports)
                    {
                        CommMessage csndMsgs = new CommMessage(CommMessage.MessageType.close);
                        csndMsgs.command = "show";
                        csndMsgs.author = "Vishal Parekh";
                        csndMsgs.to = "http://localhost:" + port.ToString() + "/IPluggableComm";
                        csndMsgs.from = "http://localhost:8085/IPluggableComm";
                        c.postMessage(csndMsgs);

                    }
                    foreach (int pID in processID)
                    {
                        KillProcessByID(pID);
                    }
                    Process.GetCurrentProcess().Kill();
                }
                c1.show();
            }
        }

        //selects the child builder from ready queue

        int selectChildPort()
        {
            int x = ready.deQ();
            return x;
        }

        //creates a child builder and open communication on given port through WCF

        static bool createProcess(int portNumber)
        {
            Process p = new Process();
            string fileName = "..\\..\\..\\ChildBuilder\\bin\\debug\\ChildBuilder.exe";
            string absFileSpec = Path.GetFullPath(fileName);
            Console.WriteLine(absFileSpec);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = portNumber.ToString();
            try
            {
                p.StartInfo.FileName = fileName;
                p.StartInfo.Arguments = commandline;
                p.Start();
                processID.Add(p.Id);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Mother Builder - Listening at port: 8080");
            Console.WriteLine("\n======================================");

            for (int i = 0; i< (Int32.Parse(args[0])); i++)
            {
                if(createProcess(i+8082))
                {
                    Console.WriteLine("\n Child Builder created successfully!");
                }
            }

            try
            {
                MotherBuilder mb = new MotherBuilder();
                mb.processQueue();
                mb.receiveMessages();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }
}
