using System;
using System.ServiceModel;

namespace DynAjax.Communication.Interfaces
{
  [ServiceContract]
  public interface IRemoteInvoke
  {
    [OperationContract]
    string RemoteInvoke(string functionName, object[] args);
  }
}
