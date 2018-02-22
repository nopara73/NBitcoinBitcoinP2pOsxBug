using NBitcoin;
using NBitcoin.Protocol;
using NBitcoin.Protocol.Behaviors;
using System;
using System.IO;
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
                    MinVersion = ProtocolVersion.SENDHEADERS_VERSION
                })
            {
                NodeConnectionParameters = connectionParameters
            };

            Console.WriteLine("Start connecting to nodes...");
            nodes.Connect();

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
                Console.WriteLine("DID NOT FIND A CONNECTION");
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
