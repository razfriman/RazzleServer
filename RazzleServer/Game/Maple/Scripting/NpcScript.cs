using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting
{
    public class NpcScript : ScriptBase
    {
        private Npc Npc;
        private string Text;
        private WaitableResult<int> mResult;

        public NpcScript(Npc npc, Character character)
            : base(ScriptType.Npc, npc.MapleID.ToString(), character) // TODO: Use actual npc script instead of ID.
        {
            Npc = npc;
        }

        public void SetResult(int value)
        {
            mResult.Set(value);
        }

        private void AddText(string text)
        {
            Text += text;
        }

        private int SendOk()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.Standard, 0, 0));
            Text = string.Empty;
            mResult.Wait();
            return mResult.Value;
        }

        private int SendNext()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.Standard, 0, 1));
            Text = string.Empty;

            mResult.Wait();

            return mResult.Value;
        }

        private int SendBackOk()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.Standard, 1, 0));
            Text = string.Empty;
            mResult.Wait();

            return mResult.Value;
        }

        private int SendBackNext()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.Standard, 1, 1));
            Text = string.Empty;
            mResult.Wait();
            return mResult.Value;
        }

        private int AskYesNo()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.YesNo));
            Text = string.Empty;
            mResult.Wait();
            return mResult.Value;
        }

        private int AskAcceptDecline()
        {
            mResult = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(Text, NpcMessageType.AcceptDecline));
            Text = string.Empty;
            mResult.Wait();
            return mResult.Value;
        }

        private void AskChoice()
        {

        }
    }
}
