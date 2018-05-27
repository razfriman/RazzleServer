using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting
{
    public class NpcScript : ScriptBase
    {
        private Npc _npc;
        private string _text;
        private WaitableResult<int> _mResult;

        public NpcScript(Npc npc, Character character)
            : base(ScriptType.Npc, npc.MapleId.ToString(), character) // TODO: Use actual npc script instead of Id.
        {
            _npc = npc;
        }

        public void SetResult(int value)
        {
            _mResult.Set(value);
        }

        private void AddText(string text)
        {
            _text += text;
        }

        private int SendOk()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 0, 0));
            _text = string.Empty;
            _mResult.Wait();
            return _mResult.Value;
        }

        private int SendNext()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 0, 1));
            _text = string.Empty;

            _mResult.Wait();

            return _mResult.Value;
        }

        private int SendBackOk()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 1, 0));
            _text = string.Empty;
            _mResult.Wait();

            return _mResult.Value;
        }

        private int SendBackNext()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 1, 1));
            _text = string.Empty;
            _mResult.Wait();
            return _mResult.Value;
        }

        private int AskYesNo()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.YesNo));
            _text = string.Empty;
            _mResult.Wait();
            return _mResult.Value;
        }

        private int AskAcceptDecline()
        {
            _mResult = new WaitableResult<int>();
            Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.AcceptDecline));
            _text = string.Empty;
            _mResult.Wait();
            return _mResult.Value;
        }

        private void AskChoice()
        {

        }
    }
}
