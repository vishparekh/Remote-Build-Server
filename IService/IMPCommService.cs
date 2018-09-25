/////////////////////////////////////////////////////////////////////
// IMPCommService.cs - service interface for MessagePassingComm    //
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
 * Added references to:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 */
/*
 * This package provides:
 * ----------------------
 * - ClientEnvironment   : client-side path and address
 * - ServiceEnvironment  : server-side path and address
 * - IMessagePassingComm : interface used for message passing and file transfer
 * - CommMessage         : class representing serializable messages
 * 
 * Required Files:
 * ---------------
 * - IPCommService.cs         : Service interface and Message definition
 * 
 * Maintenance History:
 * --------------------
 * ver 2.0 : 19 Oct 2017
 * - renamed namespace and ClientEnvironment
 * - added verbose property to ClientEnvironment
 * ver 1.0 : 15 Jun 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;

namespace MessagePassingComm
{
  using Command = String;             // Command is key for message dispatching, e.g., Dictionary<Command, Func<bool>>
  using EndPoint = String;            // string is (ip address or machine name):(port number)
  using Argument = String;      
  using ErrorMessage = String;

  public struct ClientEnvironment
  {
    public const string fileStorage = "../../../Repository";
    public const long blockSize = 1024;
    public static string baseAddress { get; set; }
    public static bool verbose { get; set; }
    public static string endPoint { get; set; } = "http://localhost:8090/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8090;
    }

  public struct ServiceEnvironment
  {
    public const string fileStorage = "../../../ServiceFileStore";
    public static string baseAddress { get; set; }
    public const long blockSize = 1024;
    public static string endPoint { get; set; } = "http://localhost:8080/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8080;
    public static bool verbose { get; set; } = false;
    }

  [ServiceContract(Namespace = "MessagePassingComm")]
  public interface IMessagePassingComm
  {
    /*----< support for message passing >--------------------------*/

    [OperationContract(IsOneWay = true)]
    void postMessage(CommMessage msg);

    // private to receiver so not an OperationContract
    CommMessage getMessage();

    /*----< support for sending file in blocks >-------------------*/

    [OperationContract]
    bool openFileForWrite(string name, string toStorage);

    [OperationContract]
    bool writeFileBlock(byte[] block);

    [OperationContract(IsOneWay = true)]
    void closeFile();
  }

  [DataContract]
  public class CommMessage
  {
    public enum MessageType
    {
      [EnumMember]
      connect,           // initial message sent on successfully connecting
      [EnumMember]
      request,           // request for action from receiver
      [EnumMember]
      reply,             // response to a request
      [EnumMember]
      closeSender,       // close down client
      [EnumMember]
      closeReceiver,      // close down server for graceful termination
      [EnumMember]
      ready,              // represents the builder is ready
      [EnumMember]
      buildRequest,       // represents the build request
      [EnumMember]
      testRequest,       // represents the test request
      [EnumMember]
      fileTransfer,       // represents the file transfer
      [EnumMember]
      close               // represents the closing of process
        }

    /*----< constructor requires message type >--------------------*/

    public CommMessage(MessageType mt)
    {
      type = mt;
    }
    /*----< data members - all serializable public properties >----*/

    [DataMember]
    public MessageType type { get; set; } = MessageType.connect;

    [DataMember]
    public string to { get; set; }

    [DataMember]
    public string from { get; set; }

    [DataMember]
    public string author { get; set; }

    [DataMember]
    public List<string> files { get; set; }

    [DataMember]
    public string body { get; set; }
     
    [DataMember]
    public Command command { get; set; }

    [DataMember]
    public List<Argument> arguments { get; set; } = new List<Argument>();

    [DataMember]
    public int threadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

    [DataMember]
    public ErrorMessage errorMsg { get; set; } = "no error";

    public void show()
    {
      Console.Write("\n  CommMessage:");
      Console.Write("\n    MessageType : {0}", type.ToString());
      Console.Write("\n    to          : {0}", to);
      Console.Write("\n    from        : {0}", from);
      Console.Write("\n    author      : {0}", author);
      Console.Write("\n    body      : {0}", body);
      Console.Write("\n    arguments   :");
      if (arguments.Count > 0)
        Console.Write("\n      ");
      foreach (string arg in arguments)
        Console.Write("{0} ", arg);
      Console.Write("\n    ThreadId    : {0}", threadId);
      Console.Write("\n    errorMsg    : {0}\n", errorMsg);
    }

    public CommMessage clone()
    {
        CommMessage msg = new CommMessage(MessageType.request);
        msg.type = type;
        msg.to = to;
        msg.from = from;
        msg.author = author;
        msg.command = command;
        foreach (string arg in arguments)
            msg.arguments.Add(arg);
        return msg;
    }
  }
}
