using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Skills;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterSkillMacros : MapleKeyedCollection<int, SkillMacro>
    {
        public Character Parent { get; }

        public CharacterSkillMacros(Character parent)
        {
            Parent = parent;
        }

        public override int GetKey(SkillMacro item) => item.Id;

        public void Save()
        {
            
        }

        public void Load()
        {
            
        }

        public void Send()
        {
            using (var pw = new PacketWriter(ServerOperationCode.SkillMacro))
            {
                pw.WriteByte(Count);

                foreach (var item in Values)
                {
                    pw.WriteString(item.Name);
                    pw.WriteBool(item.IsShout);
                    pw.WriteInt(item.Skill1);
                    pw.WriteInt(item.Skill2);
                    pw.WriteInt(item.Skill3);
                }

                Parent.Client.Send(pw);
            }
        }
    }
}
