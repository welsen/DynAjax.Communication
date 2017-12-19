using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace DynAjax.Communication
{
  public class HttpWcfClient<T> : IDisposable
  {
    public enum SupportedBindings
    {
      BasicHttp,
      BasicHttps
    }

    static readonly TimeSpan DefaultOperationTimeOut = new TimeSpan(0, 2, 0);
    public ChannelFactory<T> Factory { get; set; }
    public T Service { get; set; }
    public Binding ClientBinding { get; set; }
    public static HttpWcfClient<T> Create(string serviceUri)
    {
      var uri = new Uri(serviceUri);
      var bindingType = SupportedBindings.BasicHttp;
      switch (uri.Scheme.ToLowerInvariant())
      {
        case "https":
          bindingType = SupportedBindings.BasicHttps;
          break;
        default:
          break;
      }
      return FactoryClient(CreateBinding(bindingType, DefaultOperationTimeOut), serviceUri, new object[0]);
    }
    public static HttpWcfClient<T> Create(SupportedBindings bindingType, string endPointFormat, params Object[] args)
    {
      return FactoryClient(CreateBinding(bindingType, DefaultOperationTimeOut), endPointFormat, args);
    }
    public static HttpWcfClient<T> Create(Binding clientBinding, string endPointFormat, params Object[] args)
    {
      return FactoryClient(clientBinding, endPointFormat, args);
    }

    private static Binding CreateBinding(SupportedBindings bindingType, TimeSpan operationTimeout)
    {
      Binding clientBinding = null;
      switch (bindingType)
      {
        case SupportedBindings.BasicHttp:
          clientBinding = new BasicHttpBinding
          {
            MaxReceivedMessageSize = int.MaxValue,
            MaxBufferSize = int.MaxValue,
            MaxBufferPoolSize = int.MaxValue,
            ReaderQuotas =
            {
              MaxArrayLength = int.MaxValue,
              MaxBytesPerRead = int.MaxValue,
              MaxNameTableCharCount = int.MaxValue,
              MaxDepth = int.MaxValue
            }
          };
          break;
        case SupportedBindings.BasicHttps:
          clientBinding = new BasicHttpsBinding
          {
            MaxReceivedMessageSize = int.MaxValue,
            MaxBufferSize = int.MaxValue,
            MaxBufferPoolSize = int.MaxValue,
            ReaderQuotas =
            {
              MaxArrayLength = int.MaxValue,
              MaxBytesPerRead = int.MaxValue,
              MaxNameTableCharCount = int.MaxValue,
              MaxDepth = int.MaxValue
            }
          };
          break;
        default:
          break;
      }
      if (operationTimeout != null)
      {
        clientBinding.SendTimeout = operationTimeout;
      }
      return clientBinding;
    }

    private static HttpWcfClient<T> FactoryClient(Binding clientBinding, string endPointFormat, params Object[] args)
    {
      if (endPointFormat == null || args == null) throw new ArgumentNullException((endPointFormat == null) ? "format" : "args");

      var endPoint = string.Format(endPointFormat, args);

      var clientEndpoint = new EndpointAddress(endPoint);

      var rv = new HttpWcfClient<T>()
      {
        ClientBinding = clientBinding
      };
      try
      {
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
          // Handle the timeout exception
          ((IChannel)Service).Abort();
        }
        catch (CommunicationException /* communicationException */)
        {
          // Handle the communication exception
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
          // Handle the timeout exception
          Factory.Abort();
        }
        catch (CommunicationException /* communicationException */)
        {
          // Handle the communication exception
          Factory.Abort();
        }
      }
    }
    #endregion
  }
}