using NBitcoin;
using NBitcoin.Protocol;
using NBitcoin.Protocol.Behaviors;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinP2pOsxBug
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionParameters = new NodeConnectionParameters();

            var addressManagerFilePath = "AddressManager.dat";
            AddressManager addressManager;
            try
            {
                addressManager = AddressManager.LoadPeerFile(addressManagerFilePath);
                Console.WriteLine($"Loaded {nameof(AddressManager)} from `{addressManagerFilePath}`.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"{nameof(AddressManager)} did not exist at `{addressManagerFilePath}`. Initializing new one.");
                addressManager = new AddressManager();
            }

            connectionParameters.TemplateBehaviors.Add(new AddressManagerBehavior(addressManager));

            var nodes = new NodesGroup(Network.Main, connectionParameters,
                new NodeRequirement
                {
                    RequiredServices = NodeServices.Network,
                    MinVersion = ProtocolVersion.WITNESS_VERSION
                });

            connectionParameters.ReceiveBufferSize = 1024;
            connectionParameters.SendBufferSize = 1024;

            Console.WriteLine("Start connecting to nodes...");
            nodes.Connect();

            Console.WriteLine();
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.AddressFrom)}: {connectionParameters.AddressFrom}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.IsRelay)}: {connectionParameters.IsRelay}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.PreferredTransactionOptions)}: {connectionParameters.PreferredTransactionOptions}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.ReceiveBufferSize)}: {connectionParameters.ReceiveBufferSize}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.ReuseBuffer)}: {connectionParameters.ReuseBuffer}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.SendBufferSize)}: {connectionParameters.SendBufferSize}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.Services)}: {connectionParameters.Services}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.TemplateBehaviors)}.Count(): {connectionParameters.TemplateBehaviors.Count()}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.UserAgent)}: {connectionParameters.UserAgent}");
            Console.WriteLine($"{nameof(connectionParameters)}.{nameof(connectionParameters.Version)}: {connectionParameters.Version}");
            Console.WriteLine();

            Console.WriteLine($"{nameof(addressManager)}.{nameof(addressManager.Count)}: {addressManager.Count}");
            Console.WriteLine($"{nameof(addressManager)}.GetAddr().Count(): {addressManager.GetAddr().Count()}");
            Console.WriteLine($"{nameof(addressManager)}.GetSerializedSize(): {addressManager.GetSerializedSize()}");
            Console.WriteLine();

            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.AllowSameGroup)}: {nodes.AllowSameGroup}");
            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.ConnectedNodes)}.{nameof(nodes.ConnectedNodes.Count)}: {nodes.ConnectedNodes.Count}");
            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.MaximumNodeConnection)}: {nodes.MaximumNodeConnection}");
            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.Requirements)}.{nameof(nodes.Requirements.MinVersion)}: {nodes.Requirements.MinVersion}");
            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.Requirements)}.{nameof(nodes.Requirements.RequiredServices)}: {nodes.Requirements.RequiredServices}");
            Console.WriteLine($"{nameof(nodes)}.{nameof(nodes.Requirements)}.{nameof(nodes.Requirements.SupportSPV)}: {nodes.Requirements.SupportSPV}");
            Console.WriteLine();

            try
            {
                int i = 0;
                while (i < 42) // 7 minutes
                {
                    Console.WriteLine($"Witing for a connection for {i / (double)6} minutes....");
                    if (nodes.ConnectedNodes.Count >= 1)
                    {
                        Console.WriteLine("SUCCESSFULLY FOUND A CONNECTION");
                        return;
                    }
                    Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                    i++;
                }
                throw new TimeoutException($"DID NOT FIND A CONNECTION within {i / (double)6} minutes.");
            }
            finally
            {
                Console.WriteLine($"Saving {nameof(AddressManager)} to `{addressManagerFilePath}`.");
                addressManager.SavePeerFile(addressManagerFilePath, Network.Main);
                nodes.Dispose();
            }
        }
    }
}
