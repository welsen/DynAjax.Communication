using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;

using DynAjax.Communication.Interfaces;

namespace DynAjax.Communication.Attributes
{
  [AttributeUsage(AttributeTargets.Method)]
  public class InvokeService : Attribute
  {
    public string HostName { get; set; }
    public int Port { get; set; }
    public string EndPoint { get; set; }
    public bool UseSSL { get; set; }

    public InvokeService()
    {
      HostName = "localhost";
      Port = 4200;
      EndPoint = "Service";
      UseSSL = false;
    }

    public InvokeService(string endPoint) : base() => EndPoint = endPoint;
    public InvokeService(string hostName, int port) : base()
    {
      HostName = hostName;
      Port = port;
    }
    public InvokeService(string hostName, int port, bool useSsl) : base()
    {
      HostName = hostName;
      Port = port;
      UseSSL = useSsl;
    }
    public InvokeService(string hostName, int port, string endPoint) : base()
    {
      HostName = hostName;
      Port = port;
      EndPoint = endPoint;
    }
    public InvokeService(string hostName, int port, bool useSsl, string endPoint) : base()
    {
      HostName = hostName;
      Port = port;
      EndPoint = endPoint;
      UseSSL = useSsl;
    }

    public T RemoteInvokeNetTcp<R, T>(string functionName, params object[] args) where R : ICommunicationObject, IRemoteInvoke
    {
      T result;

      using (var cc = NetTcpWcfClient<R>.Create($@"net.tcp://{HostName}:{Port}/{EndPoint}/"))
      {
        result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(cc.Service.RemoteInvoke(functionName, args));
      }

      return result;
    }
    public T RemoteInvokeHttp<R, T>(string functionName, params object[] args) where R : ICommunicationObject, IRemoteInvoke
    {
      T result;

      using (var cc = HttpWcfClient<R>.Create($@"{(UseSSL ? "https" : "http")}://{HostName}:{Port}/{EndPoint}/"))
      {
        result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(cc.Service.RemoteInvoke(functionName, args));
      }

      return result;
    }
  }
}
