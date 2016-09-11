using System.Text;
//using System.Xml;

namespace RazzleServer
{
    public class MapleSharkConfigCreator
    {
        public string GenerateConfigFile()
        {
            var buffer = new StringBuilder();
            //var writer = XmlWriter.Create(buffer);

            return buffer.ToString();
        }
    }
}

/*
<ArrayOfDefinition
    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"
    xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">

 <Definition>
     <Build>ServerConstants.Version</Build>        
     <Locale>8</Locale>
     <Outbound>true</Outbound>
     <Opcode>op</Opcode>
     <Name>Enum.GetName(typeof(RecvHeader), op)</Name>
     <Ignore>false</Ignore>"
 </Definition>
                    */