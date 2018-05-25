using System;
using System.Text;
using System.Xml;
using RazzleServer.Center;
using RazzleServer.Common.Packet;

namespace RazzleServer.Common.Util
{
    public class MapleSharkConfigCreator
    {
        public string GenerateConfigFile()
        {
            var buffer = new StringBuilder();
            using (var writer = XmlWriter.Create(buffer))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ArrayOfDefinition");

                foreach (var value in Enum.GetValues(typeof(ClientOperationCode)))
                {
                    writer.WriteStartElement("Definition");

                    writer.WriteElementString("Build", ServerConfig.Instance.Version.ToString());
                    writer.WriteElementString("Locale", "8");
                    writer.WriteElementString("Outbound", "false");
                    writer.WriteElementString("Opcode", ((ushort) value).ToString());
                    writer.WriteElementString("Name", value.ToString());
                    writer.WriteElementString("Ignore", "false");

                    writer.WriteEndElement();
                }

                foreach (var value in Enum.GetValues(typeof(ServerOperationCode)))
                {
                    writer.WriteStartElement("Definition");

                    writer.WriteElementString("Build", ServerConfig.Instance.Version.ToString());
                    writer.WriteElementString("Locale", "8");
                    writer.WriteElementString("Outbound", "true");
                    writer.WriteElementString("Opcode", ((ushort)value).ToString());
                    writer.WriteElementString("Name", value.ToString());
                    writer.WriteElementString("Ignore", "false");

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return buffer.ToString();
        }
    }
}