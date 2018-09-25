//////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client prototype GUI for Pluggable Repository   // 
// version: 1.0                                                         //
// Language:    C#, Visual Studio 2017                                  //
// Platform:    Dell Inspiron 15, Windows 10                            //
// Application: Remote Build Server Prototypes                          //
//                                                                      //
// Source: Dr. Jim Fawcett                                              //
// Author Name : Vishal Parekh                                          //
// CSE681: Software Modeling and Analysis, Fall 2017                    //
/////////////////////////////////////////////////////////////////////////
/*  
 *  Purpose:
 *    Prototype for a client for the Pluggable Repository.This application
 *    doesn't connect to the repository - it has no Communication facility.
 *    It simply explores the kinds of user interface elements needed for that.
 *
 *  Required Files:
 *    MainWindow.xaml, MainWindow.xaml.cs - view into repository and checkin/checkout
 *    Window1.xaml, Window1.xaml.cs       - Code and MetaData view for individual packages
 *
 * Public Interface:
 *    void initializeFilesListBox() - it initializes first tab  list box with the files in directory
 *    initializeTestDriverListBox() - it initializes second tab list box with all the test driver files
 *    void sendButton_Click(object sender, RoutedEventArgs e) - performs operations when sender button is clicked
 *    SelectTestButton_Click(object sender, RoutedEventArgs e) - performs operations when create test request button is clicked
 *    Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) - performs operations before closing window
 *    SelectTestButton_Click(object sender, RoutedEventArgs e) - perorms operations when add test button is clicked 
 *    testDriverListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) - performs operations when anything in test files list box in first tab is double clicked
 *    void initializeTestLogListBox() - Intializes the test log files
 *    void RequestFilesButton_Click(object sender, RoutedEventArgs e) - Requests files from the repository
 *    void Refresh_Click(object sender, RoutedEventArgs e) - Refreshes the build log files
 *    void Test_Refresh_Click(object sender, RoutedEventArgs e) - Refreshes the test log files
 *    
 *  Maintenance History:
 *    ver 1.0 : 15 Jun 2017
 *    - first release
 */
using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PluggableRepoClient
{

    public partial class MainWindow : Window
    {
        List<Window1> popups = new List<Window1>();
        List<TestElement> testElements = new List<TestElement>();
        Random r = new Random();
        Comm comm = new Comm("http://localhost", 8090);

        //it initializes first tab  list box with the files in directory

        void initializeFilesListBox()
        {
            filesListBox.Items.Clear();
            List<string> files = getListOfFiles("../../../Repository/");
            foreach (string file in files)
            {
                if (file.Contains(".cs") && !file.Contains("Driver"))
                    filesListBox.Items.Add(file);
            }
            statusLabel.Text = "Action: Show file text by double clicking on file";
        }

        //it initializes second tab list box with all the test driver files

        void initializetestDriverListBox()
        {
            testDriverListBox.Items.Clear();
            List<string> files = getListOfFiles("../../../Repository/");
            foreach (string file in files)
            {
                if (file.Contains("Driver") && file.Contains(".cs"))
                    testDriverListBox.Items.Add(file);
            }
        }

        //retrieves files from the specified directory

        public List<string> getListOfFiles(string path)
        {
            string[] listOfFiles = Directory.GetFiles(path);
            int index = 0;
            foreach (string file in listOfFiles)
            {
                listOfFiles[index] = System.IO.Path.GetFileName(listOfFiles[index]);
                index++;
            }
            return listOfFiles.ToList<string>();
        }

        //returns list of test drivers

        private void getTestDriverList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codeWindow = new Window1();
            codeWindow.Show();
            popups.Add(codeWindow);
            codeWindow.codeView.Blocks.Clear();
            string file = (string)testDriverListBox.SelectedItem;
            codeWindow.codeLabel.Text = "Code View: " + file;
            showFileContents(file, codeWindow);
            return;
        }

        //shows the file contents

        private void showFileContents(string file, Window1 window)
        {
            string path = System.IO.Path.Combine("../../../Repository/", file);
            Paragraph paragraph = new Paragraph();
            string fileText = "";
            try
            {
                fileText = System.IO.File.ReadAllText(path);
            }
            finally
            {
                paragraph.Inlines.Add(new Run(fileText));
            }
            window.codeView.Blocks.Clear();
            window.codeView.Blocks.Add(paragraph);
        }

        //initializes the window

        public MainWindow()
        {
            InitializeComponent();
        }

        //initializes all the component

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            initializeTestListBox();
            initializeLogListBox();
            initializeTestLogListBox();

        }

        //performs operations when add test button is clicked 

        private void SelectTestButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)testDriverListBox.SelectedItem == null || filesListBox.SelectedItems.Count == 0)
                statusLabel.Text = "Status: Please select Test Driver file and Code to test files";
            else
            {
                TestElement testElementObj = new TestElement();
                testElementObj.testDriver = (string)testDriverListBox.SelectedItem;

                foreach (string selectedItem in filesListBox.SelectedItems)
                    testElementObj.addCode(selectedItem);

                testElements.Add(testElementObj);
                statusLabel.Text = "Status: Test Added Successfully";
                TestRequest.IsEnabled = true;
                testDriverListBox.UnselectAll();
                filesListBox.UnselectAll();
            }
        }

        //creates mother builder process

        static bool createProcess(string numberOfProcesses)
        {
            Process p = new Process();
            string fileName = "..\\..\\..\\MotherBuilder\\bin\\debug\\MotherBuilder.exe";
            string absFileSpec = System.IO.Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = numberOfProcesses;
            try
            {

                Process.Start("..\\..\\..\\Client\\bin\\debug\\Client.exe", commandline);
                Process.Start("..\\..\\..\\MotherBuilder\\bin\\debug\\MotherBuilder.exe", commandline);
                Process.Start("..\\..\\..\\TestHarness\\bin\\debug\\TestHarness.exe", commandline);

            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        //performs operations when start builder button is clicked 

        private void InitiateBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            if (noOfProcessesTextBox.Text == "")
                statusLabel.Text = "Status: Please choose more than 0 child builders";
            else
            {
                if (Convert.ToInt32(noOfProcessesTextBox.Text) > 10 || Convert.ToInt32(noOfProcessesTextBox.Text) <= 0)
                    statusLabel.Text = "Status: Please choose less than 20 child builders";
                else
                {
                    if (createProcess(noOfProcessesTextBox.Text))
                    {
                        Console.Write(" Child Builder created successfully");
                    }
                    else
                    {
                        Console.Write(" Failure occured");
                    }
                }
            }
        }

        //performs operations when create test request button is clicked
        private void CreateTestRequestButton_Click(object sender, RoutedEventArgs e)
        {
            BuildRequest tr = new BuildRequest();
            tr.author = "Vishal Parekh";
            tr.dateTime = DateTime.Now.ToString();
            tr.tests.AddRange(testElements);
            string fileName = "BuildRequest_" + r.Next(1, 100) + ".xml";
            string savePath = "../../../Repository/" + fileName;
            tr.makeRequest();
            tr.saveXml(savePath);
            testListBox.Items.Clear();
            initializeTestListBox();
            testElements.Clear();
            statusLabel.Text = "Status: Test Request Created with name " + fileName + savePath;
        }

        //initialized the list box with all test request

        void initializeTestListBox()
        {

            String pattern = "BuildRequest*";
            List<string> files = filesWithPattern("../../../Repository/", pattern);
            foreach (string file in files)
            {
                testListBox.Items.Add(file);
            }

            statusLabel.Text = "Action: To see the content of file, double click on that";
        }

        //returns the files of given pattern

        public List<string> filesWithPattern(String path, String pattern)
        {
            string[] files = Directory.GetFiles(path, pattern);
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);
            }
            return files.ToList<string>();
        }

        //performs operations when code button is clicked

        private void showCodeButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);
        }

        //returns list of files when moue double click operation is performed

        private void getListOfFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
            popups.Add(window);
        }

        //closes the window

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var popup in popups)
                popup.Close();
        }

        //performs operations when anything in test list box in second tab is double clicked 

        private void testListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
            popups.Add(window);
            window.codeView.Blocks.Clear();
            string file = (string)testListBox.SelectedItem;
            window.codeLabel.Text = "Source code: " + file;
            showFileContents(file, window);
            return;
        }

        //performs operations when stop builder button is clicked 

        private void StopBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.close);
            csndMsg.command = "show";
            csndMsg.author = "Vishal Parekh";
            csndMsg.to = "http://localhost:8080/IPluggableComm";
            csndMsg.from = "http://localhost:8085/IPluggableComm";
            comm.postMessage(csndMsg);
        }

        //refreshes the build log file contents

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            logListBox.Items.Clear();
            String pattern = "BuildLog*";
            List<string> files = filesWithPattern("../../../Repository/", pattern);
            foreach (string file in files)
            {
                logListBox.Items.Add(file);
            }
        }

        //refreshes the test log file contents

        private void Test_Refresh_Click(object sender, RoutedEventArgs e)
        {
            testLogListBox.Items.Clear();
            String pattern = "TestLog*";
            List<string> files = filesWithPattern("../../../Repository/", pattern);
            foreach (string file in files)
            {
                testLogListBox.Items.Add(file);
            }
        }

        //function for viewing the logs

        private void logListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)logListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }

        //function for viewing the logs

        private void testLogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)testLogListBox.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }

        //Shows file in other window

        private void showFile(string fileName, Window1 popUp)
        {
            string path = System.IO.Path.Combine("../../../Repository/", fileName);
            Paragraph paragraph = new Paragraph();
            string fileText = "";
            try
            {
                fileText = System.IO.File.ReadAllText(path);
            }
            finally
            {
                paragraph.Inlines.Add(new Run(fileText));
            }

            // add code text to code view
            popUp.codeView.Blocks.Clear();
            popUp.codeView.Blocks.Add(paragraph);
        }

        //initialize list box in fourth tab

        private void initializeLogListBox()
        {
            logListBox.Items.Clear();
            String pattern = "BuildLog*";
            List<string> files = filesWithPattern("../../../Repository/", pattern);
            foreach (string file in files)
            {
                logListBox.Items.Add(file);
            }
            statusLabel.Text = "Hit refresh to view recent build logs";
        }

        //intialize test logs

        private void initializeTestLogListBox()
        {
            testLogListBox.Items.Clear();
            String pattern = "TestLog*";
            List<string> files = filesWithPattern("../../../Repository/", pattern);
            foreach (string file in files)
            {
                testLogListBox.Items.Add(file);
            }
            statusLabel.Text = "Hit refresh to view recent test logs";
        }

        //request files from repository to display

        private void RequestFilesButton_Click(object sender, RoutedEventArgs e)
        {
            initializeFilesListBox();
            initializetestDriverListBox();
        }
    }
}
