//////////////////////////////////////////////////////////////////////////////////////////
// Logger.cs : Creates a log file and each errors and warings are logged in the file    //
// version: 1.0                                                                         //
// Language:    C#, Visual Studio 2017                                                  //
// Platform:    Dell Inspiron 15, Windows 10                                            //
// Application: Remote Build Server Prototypes                                          //
//                                                                                      //
// Source: Dr. Jim Fawcett                                                              //
// Author Name : Vishal Parekh                                                          //
// CSE681: Software Modeling and Analysis, Fall 2017                                    //
//////////////////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This Package handles the creation of log file and logging all the build and test logs 
 * 
 * Public Interface:
 * ================= 
 * void ErrorLog(string ErrorMessage) : Logs the errormessage into the log file
 * 
 * Build Process:
 * --------------
 * Required Files: Logger.cs 
   Build Command: csc Logger.cs

 * Maintenance History:
    - Version 1.0 Oct 2017 
 * --------------------*/



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuildServer
{
    public class Logger
    {
        private string sLogFormat;
        private string sErrorTime;
        string path;

        public Logger(string p)
        {
            path = p;
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }

        //makes an entry in the log life
        public void ErrorLog(string sErrMsg)
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.Flush();
            sw.Close();
        }

    }
#if (TEST_LOGGER)
    class LoggerTest
    {
        static void Main(string[] args)
        {
            Logger log = new Logger("..//..//..//Repository/log_8080.txt");
            log.ErrorLog("Build Successfull");  
            Console.Write("Log file generated");
        }
    }
#endif

}
