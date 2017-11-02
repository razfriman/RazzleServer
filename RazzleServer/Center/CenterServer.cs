using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Packet;
using RazzleServer.Server;

namespace RazzleServer.Center
{
    public class CenterServer : MapleServer<CenterClient>
    {
        public CenterClient Login { get; set; }
        public Worlds Worlds { get; private set; }
        public Maple.Migrations Migrations { get; private set; }
        public Dictionary<InteroperabilityOperationCode, List<CenterPacketHandler>> PacketHandlers { get; private set; } = new Dictionary<InteroperabilityOperationCode, List<CenterPacketHandler>>();

        public CenterServer()
        {
            Worlds = new Worlds();
            Migrations = new Maple.Migrations();
            Start(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
        }

        public override void RegisterPacketHandlers()
        {
            var types = Assembly.GetEntryAssembly().GetTypes();

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes()
                                     .OfType<InteroperabilityPacketHandlerAttribute>()
                                     .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<CenterPacketHandler>();
                    }

                    handlerCount++;
                    var handler = (CenterPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.LogDebug($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }
    }
}
