using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace LiteDbExplorer
{
    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs args);

    public class CommandExecutedEventArgs : EventArgs
    {
        public string Command
        {
            get; set;
        }

        public string Args
        {
            get; set;
        }

        public CommandExecutedEventArgs()
        {
        }

        public CommandExecutedEventArgs(string command, string args)
        {
            Command = command;
            Args = args;
        }
    }

    [ServiceContract]
    public interface IPipeService
    {
        [OperationContract]
        void InvokeCommand(string command, string args);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class PipeService : IPipeService
    {
        public event CommandExecutedEventHandler CommandExecuted;

        public void InvokeCommand(string command, string args)
        {
            CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, args));
        }
    }

    public class PipeServer
    {
        private readonly string _endpoint;

        public PipeServer(string endpoint)
        {
            _endpoint = endpoint;
        }

        public void StartServer(IPipeService service)
        {
            var serviceHost = new ServiceHost(service, new Uri(_endpoint));
            serviceHost.AddServiceEndpoint(typeof(IPipeService), new NetNamedPipeBinding(), "LiteDBExplorerService");
            serviceHost.Open();
        }
    }

    public class PipeClient : ClientBase<IPipeService>
    {
        public PipeClient(string endpoint)
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IPipeService)), new NetNamedPipeBinding(), new EndpointAddress(endpoint.TrimEnd('/') + @"/LiteDBExplorerService")))
        {
        }

        public void InvokeCommand(string command, string args)
        {
            Channel.InvokeCommand(command, args);
        }
    }
}
