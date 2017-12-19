using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;

using DynAjax.Communication.Interfaces;

namespace DynAjax.Communication
{
  public static class RemoteInvoker
  {
    // [InvokeService] 
    // [MethodImpl(MethodImplOptions.NoInlining)]
    public static T InvokeNetTcp<R, T>(string hostName, int port, string endPoint, string functionName, params object[] args) where R : ICommunicationObject, IRemoteInvoke
    {
      // MethodBase method = MethodBase.GetCurrentMethod();
      // InvokeService serviceAttrs = (InvokeService)method.GetCustomAttributes(typeof(InvokeService), true)[0];
      // string hostName = serviceAttrs.HostName;
      // int port = serviceAttrs.Port;
      // string endPoint = serviceAttrs.EndPoint;

      T result;

      // using (var cc = NetTcpWcfClient<R>.Create($@"net.tcp://{cache.ServerHostName}:{cache.ServicePort}/{endPoint}/"))
      // {
      //   Type cacheServiceType = cc.Service.GetType();
      //   MethodInfo functionToInvoke = cacheServiceType.GetMethod(functionName);
      //   var functionResult = functionToInvoke.Invoke(cc.Service, BindingFlags.InvokeMethod, null, args, null);
      //   result = functionResult;
      // }

      using (var cc = NetTcpWcfClient<R>.Create($@"net.tcp://{hostName}:{port}/{endPoint}/"))
      {
        result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(cc.Service.RemoteInvoke(functionName, args));
      }

      return result;
    }

    // [InvokeService] 
    // [MethodImpl(MethodImplOptions.NoInlining)]
    public static T InvokeHttp<R, T>(string schema, string hostName, int port, string endPoint, string functionName, params object[] args) where R : ICommunicationObject, IRemoteInvoke
    {
      T result;

      using (var cc = NetTcpWcfClient<R>.Create($@"{schema}://{hostName}:{port}/{endPoint}/"))
      {
        result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(cc.Service.RemoteInvoke(functionName, args));
      }

      return result;
    }
  }
}