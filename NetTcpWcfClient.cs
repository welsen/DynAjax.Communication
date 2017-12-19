using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace DynAjax.Communication
{
  public class NetTcpWcfClient<T> : IDisposable
  {
    static readonly TimeSpan DefaultOperationTimeOut = new TimeSpan(0, 2, 0);

    public ChannelFactory<T> Factory { get; set; }
    public T Service { get; set; }
    public static NetTcpWcfClient<T> CreateDuplexReliable(object instance, TimeSpan operationTimeout, string endPointFormat, params Object[] args)
    {
      return Create(true, instance, true, operationTimeout, endPointFormat, args);
    }
    public static NetTcpWcfClient<T> CreateDuplex(object instance, TimeSpan operationTimeout, string endPointFormat, params Object[] args)
    {
      return Create(true, instance, false, operationTimeout, endPointFormat, args);
    }

    public static NetTcpWcfClient<T> CreateReliable(string endPointFormat, params Object[] args)
    {
      return Create(false, null, true, DefaultOperationTimeOut, endPointFormat, args);
    }

    public static NetTcpWcfClient<T> Create(string endPointFormat, params Object[] args)
    {
      return Create(false, null, false, DefaultOperationTimeOut, endPointFormat, args);
    }

    private static NetTcpWcfClient<T> Create(bool duplex, object instance, bool reliable, TimeSpan operationTimeout, string endPointFormat, params Object[] args)
    {
      if (duplex && instance == null) throw new ArgumentNullException(nameof(instance));
      if (endPointFormat == null || args == null) throw new ArgumentNullException((endPointFormat == null) ? "format" : "args");

      var endPoint = string.Format(endPointFormat, args);

      var clientBinding = new NetTcpBinding
      {
        MaxReceivedMessageSize = int.MaxValue,
        MaxBufferSize = int.MaxValue,
        MaxBufferPoolSize = int.MaxValue,
        ReaderQuotas =
        {
          MaxArrayLength = int.MaxValue,
          MaxBytesPerRead = int.MaxValue,
          MaxNameTableCharCount = int.MaxValue,
          MaxDepth = int.MaxValue,
          MaxStringContentLength = int.MaxValue
        },
        Security =
        {
          Mode = SecurityMode.None,
          Transport =
          {
            ClientCredentialType = TcpClientCredentialType.None,
          },
          Message =
          {
            ClientCredentialType = MessageCredentialType.None
          }
        },
      };
      if (operationTimeout != null)
      {
        clientBinding.SendTimeout = operationTimeout;
      }

      var clientEndpoint = new EndpointAddress(endPoint);

      var rv = new NetTcpWcfClient<T>();
      try
      {
        if (duplex)
          rv.Factory = new DuplexChannelFactory<T>(new InstanceContext(instance), clientBinding, clientEndpoint);
        else
          rv.Factory = new ChannelFactory<T>(clientBinding, clientEndpoint);

        rv.Service = rv.Factory.CreateChannel();
        return rv;
      }
      catch (Exception /*e*/)
      {
        throw;
      }
    }

    #region Implementation of IDisposable
    public virtual void Dispose()
    {
      CloseChannel();
      CloseFactory();
    }

    protected void CloseChannel()
    {
      if (((IChannel)Service).State == CommunicationState.Opened)
      {
        try
        {
          ((IChannel)Service).Close();
        }
        catch (TimeoutException /* timeout */)
        {
          ((IChannel)Service).Abort();
        }
        catch (CommunicationException /* communicationException */)
        {
          ((IChannel)Service).Abort();
        }
      }
    }

    protected void CloseFactory()
    {
      if (Factory.State == CommunicationState.Opened)
      {
        try
        {
          Factory.Close();
        }
        catch (TimeoutException /* timeout */)
        {
          Factory.Abort();
        }
        catch (CommunicationException /* communicationException */)
        {
          Factory.Abort();
        }
      }
    }
    #endregion
  }
}
