using System.Collections.Generic;
using System.Linq;
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

        public int State { get; set; }

        public abstract void Execute();

        public void SetResult(int value) => _result?.Set(value);

        public void SetResult(string value) => _stringResult?.Set(value);

        public void Send(NpcStateInfo state, bool addState = true)
        {
            if (addState)
            {
                States.Add(state);
            }

            Character.Client.Send(Npc.GetDialogPacket(state));
        }

        protected int SendOk(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Standard, Text = text, IsPrevious = false, IsNext = false});

            _result.Wait();
            return _result.Value;
        }

        protected int SendNext(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Standard, Text = text, IsPrevious = false, IsNext = true});

            _result.Wait();
            return _result.Value;
        }

        protected int SendBackOk(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Standard, Text = text, IsPrevious = true, IsNext = false});

            _result.Wait();
            return _result.Value;
        }

        protected int SendBackNext(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Standard, Text = text, IsPrevious = true, IsNext = true});

            _result.Wait();
            return _result.Value;
        }

        protected int AskYesNo(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.YesNo, Text = text});

            _result.Wait();
            return _result.Value;
        }

        protected int SendAcceptDecline(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.AcceptDecline, Text = text});

            _result.Wait();
            return _result.Value;
        }

        protected int SendAcceptDeclineNoExit(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.AcceptDeclineNoExit, Text = text});

            _result.Wait();
            return _result.Value;
        }

        protected int SendRequestNumber(string text, int numberDefault = 0, int numberMinimum = 0,
            int numberMaximum = 0)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo
            {
                Type = NpcMessageType.RequestNumber,
                Text = text,
                NumberDefault = numberDefault,
                NumberMinimum = numberMinimum,
                NumberMaximum = numberMaximum
            });

            _result.Wait();
            return _result.Value;
        }

        protected int SendRequestStyle(string text, params int[] styles)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.RequestStyle, Text = text, Styles = styles.ToList()});

            _result.Wait();
            return _result.Value;
        }

        protected string SendRequestText(string text)
        {
            _stringResult = new WaitableResult<string>();

            Send(new NpcStateInfo {Type = NpcMessageType.RequestText, Text = text});

            _stringResult.Wait();
            return _stringResult.Value;
        }
    }
}
