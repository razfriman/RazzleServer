using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class ANpcScript
    {
        private WaitableResult<int> _result;
        public Character Character;
        public Npc Npc;

        public abstract void Execute();

        public void SetResult(int value)
        {
            _result?.Set(value);
        }

        protected int SendOk(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.Standard, 0, 0));
            _result.Wait();
            return _result.Value;
        }

        protected int SendNext(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.Standard, 0, 1));
            _result.Wait();
            return _result.Value;
        }

        protected int SendBackOk(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.Standard, 1, 0));
            _result.Wait();

            return _result.Value;
        }

        protected int SendBackNext(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.Standard, 1, 1));
            _result.Wait();
            return _result.Value;
        }

        private int AskYesNo(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.YesNo));
            _result.Wait();
            return _result.Value;
        }

        protected int AskAcceptDecline(string text)
        {
            _result = new WaitableResult<int>();
            Character.Client.Send(Npc.GetDialogPacket(text, NpcMessageType.AcceptDecline));
            _result.Wait();
            return _result.Value;
        }

        //private void AskChoice()
        //{

        //}
    }
}
