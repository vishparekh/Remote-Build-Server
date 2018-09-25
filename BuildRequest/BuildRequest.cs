/////////////////////////////////////////////////////////////////////
// BuildRequest.cs -   This package provides  functionality        //
//                     of building test request for                //
//                     Build Server system.                        //
// version: 1                                                       //
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
 * This Package declares the structure of build request and builds the build request after the client selectes file from the repository to be tested
 * 
 * Public Interface:
 * ================= 
 * void addDriver(string name): for adding driver to test element
 * void addCode(string name): for adding test code files to test element
 * 
 * Build Process:
 * --------------
 * Required Files: BuildRequest.cs
   Build Command: csc BuildRequest.cs

 * Maintenance History:
    - Versio 1.0 Oct 2017 
 * --------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluggableRepoClient
{

    ///////////////////////////////////////////////////////////////////
    // TestElement and BuildRequest classes                         //

    /* structure about a build request */

    public class BuildRequest 
    {
        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";
        public List<TestElement> tests { get; set; } = new List<TestElement>();
        public XDocument doc { get; set; } = new XDocument();

        //build XML document that represents a build request

        public void makeRequest()
        {
            XElement testRequestElem = new XElement("testRequest");
            doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            foreach (TestElement test in tests)
            {
                XElement testElem = new XElement("test");
                testRequestElem.Add(testElem);

                string testDriver = test.testDriver;

                XElement driverElem = new XElement("testDriver");
                driverElem.Add(testDriver);
                testElem.Add(driverElem);

                foreach (string file in test.testCodes)
                {
                    XElement testedElem = new XElement("tested");
                    testedElem.Add(file);
                    testElem.Add(testedElem);
                }

            }
        }

        //save TestRequest to XML file

        public bool saveXml(string path)
        {
            try
            {
                doc.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }

        //load TestRequest from XML file 

        public bool loadXml(string path)
        {
            try
            {
                doc = XDocument.Load(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }

    }

    /* structure of a single test */

    public class TestElement  
    {
        public string testDriver { get; set; }

        public List<string> testCodes { get; set; } = new List<string>();

        public TestElement() { }
       
        //for adding driver to test element

        public void addDriver(string name)
        {
            testDriver = name;
        }

        //for adding code to be tested to test element

        public void addCode(string name)
        {
            testCodes.Add(name);
        }
       
    }

#if (TEST_TESTREQUEST)
    class TestRequestTest
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Testing TestRequest");
            Console.Write("\n =====================");

            string savePath = "../../../ClientFileStore/";
            string fileName = "BuildRequest.xml";

            if (!System.IO.Directory.Exists(savePath))
                System.IO.Directory.CreateDirectory(savePath);
            string fileSpec = System.IO.Path.Combine(savePath, fileName);
            fileSpec = System.IO.Path.GetFullPath(fileSpec);

            Console.Write("\n  reading from \"{0}\"", fileSpec);

            BuildRequest testRequestObj = new BuildRequest();
            testRequestObj.loadXml(fileSpec);
            Console.Write("\n{0}", testRequestObj.doc.ToString());
            Console.Write("\n");
            Console.Write("\n\n");
        }
    }
#endif

}

