using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class ANpcScript
    {
        private WaitableResult<int> _result;
        private WaitableResult<string> _stringResult;

        public Character Character { get; set; }

        public Npc Npc { get; set; }

        public List<NpcStateInfo> States { get; set; } = new List<NpcStateInfo>();

        public abstract void Execute();

        public void SetResult(int value)
        {
            _result?.Set(value);
        }

        public void SetResult(string value)
        {
            _stringResult?.Set(value);
        }

        protected int SendOk(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.Standard,
            Text = text,
            IsPrevious = false,
            IsNext = false
        });

        protected int SendNext(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.Standard,
            Text = text,
            IsPrevious = false,
            IsNext = true
        });

        protected int SendBackOk(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.Standard,
            Text = text,
            IsPrevious = true,
            IsNext = false
        });

        protected int SendBackNext(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.Standard,
            Text = text,
            IsPrevious = true,
            IsNext = true
        });

        protected int AskYesNo(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.YesNo,
            Text = text
        });

        protected int SendAcceptDecline(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.AcceptDecline,
            Text = text
        });

        protected int SendAcceptDeclineNoExit(string text) => Send(new NpcStateInfo
        {
            Type = NpcMessageType.AcceptDeclineNoExit,
            Text = text
        });

        private int Send(NpcStateInfo state)
        {
            _result = new WaitableResult<int>();
            States.Add(state);
            Character.Client.Send(Npc.GetDialogPacket(state));
            _result.Wait();
            return _result.Value;

        }
    }
}
