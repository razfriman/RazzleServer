using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            Character.Send(Npc.GetDialogPacket(state));
        }

        protected int SendOk(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Standard, Text = text, IsPrevious = false, IsNext = false});

            _result.Wait();
            return _result.Value;
        }

        protected int SendChoice(string text)
        {
            _result = new WaitableResult<int>();

            Send(new NpcStateInfo {Type = NpcMessageType.Choice, Text = text, IsPrevious = false, IsNext = false});

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

        public static string CreateSelectionList(NpcListType type, params int[] ids)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < ids.Length; i++)
            {
                builder.Append($"\r\n#L{i}#{MapNpcListType(type, ids[i])}#l");
            }

            return builder.ToString();
        }

        public static string Blue(string s) => $"#b{s}";
        public static string Purple(string s) => $"#d{s}";
        public static string Bold(string s) => $"#e{s}";
        public static string Green(string s) => $"#g{s}";
        public static string Black(string s) => $"#k{s}";
        public static string Red(string s) => $"#r{s}";
        public static string Backwards(string s) => $"\b{s}";
        public static string Normal(string s) => $"#n{s}";
        public static string FileRef(string s) => $"#f{s}#";
        public static string CountItem(int n) => $"#c{n}#";
        public static string PlayerRef() => "#h #";
        public static string ProgressBar(int n) => $"#B{n}#";
        public static string NpcRef(int mapleId) => $"#p{mapleId}#";
        public static string MobRef(int mapleId) => $"#o{mapleId}#";
        public static string MapRef(int mapleId) => $"#m{mapleId}#";
        public static string SkillRef(int mapleId) => $"#q{mapleId}#";
        public static string SkillIcon(int mapleId) => $"#s{mapleId}#";
        public static string ItemRef(int mapleId) => $"#t{mapleId}#";
        public static string AltItemRef(int mapleId) => $"#z{mapleId}#";
        public static string ItemIcon(int mapleId) => $"#v{mapleId}#";
        public static string InventoryItemRef(int mapleId) => $"#c{mapleId}#";

        public string GenderedText(string maleText, string femaleText) => Character.PrimaryStats.Gender == Gender.Male
            ? maleText
            : femaleText;

        public static string QuestCompleteIcon() => FileRef("UI/UIWindow.img/QuestIcon/4/0");

        public static string QuestExperienceIcon(int exp) =>
            $"{FileRef("UI/UIWindow.img/QuestIcon/8/0")} {exp} EXP";

        public static string QuestMesoIcon(int mesos) => $"{FileRef("UI/UIWindow.img/QuestIcon/4/0")} {mesos}";

        public static string QuestItemIcon(int mapleId, int quantity) => quantity == 0 || quantity == 1
            ? $"{ItemIcon(mapleId)} {ItemRef(mapleId)}"
            : $"{ItemIcon(mapleId)} {quantity} {ItemRef(mapleId)}s";

        public static string MapNpcListType(NpcListType type, int mapleId)
        {
            switch (type)
            {
                case NpcListType.Npc:
                    return NpcRef(mapleId);
                case NpcListType.Mob:
                    return MobRef(mapleId);
                case NpcListType.Map:
                    return MapRef(mapleId);
                case NpcListType.Skill:
                    return SkillRef(mapleId);
                case NpcListType.Item:
                    return MobRef(mapleId);
                case NpcListType.SkillIcon:
                    return SkillIcon(mapleId);
                case NpcListType.ItemIcon:
                    return ItemIcon(mapleId);
                case NpcListType.InventoryItem:
                    return InventoryItemRef(mapleId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
