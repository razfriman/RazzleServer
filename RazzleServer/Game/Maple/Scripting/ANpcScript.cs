using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Scripts
{
    public abstract class ANpcScript
    {
        public Character Character { get; }
        public Npc Npc { get; }

        public abstract string Name { get; }

        public abstract void Execute();

        public void SetResult(int value)
        {
            //_mResult.Set(value);
        }

        //private int SendOk(string text)
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 0, 0));
        //    _text = string.Empty;
        //    _mResult.Wait();
        //    return _mResult.Value;
        //}

        //private int SendNext()
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 0, 1));
        //    _text = string.Empty;

        //    _mResult.Wait();

        //    return _mResult.Value;
        //}

        //private int SendBackOk()
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 1, 0));
        //    _text = string.Empty;
        //    _mResult.Wait();

        //    return _mResult.Value;
        //}

        //private int SendBackNext()
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.Standard, 1, 1));
        //    _text = string.Empty;
        //    _mResult.Wait();
        //    return _mResult.Value;
        //}

        //private int AskYesNo()
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(Npc.GetDialogPacket(_text, NpcMessageType.YesNo));
        //    _text = string.Empty;
        //    _mResult.Wait();
        //    return _mResult.Value;
        //}

        //private int AskAcceptDecline()
        //{
        //    _mResult = new WaitableResult<int>();
        //    Character.Client.Send(_npc.GetDialogPacket(_text, NpcMessageType.AcceptDecline));
        //    _text = string.Empty;
        //    _mResult.Wait();
        //    return _mResult.Value;
        //}

        //private void AskChoice()
        //{

        //}
    }
}
