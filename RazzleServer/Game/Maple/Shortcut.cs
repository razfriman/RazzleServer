using RazzleServer.Common.Constants;
using RazzleServer.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Shortcut
    {
        public KeymapKey Key { get; private set; }
        public KeymapType Type { get; set; }
        public KeymapAction Action { get; set; }

        public Shortcut(KeyMapEntity entity)
        {
            Key = (KeymapKey)entity.Key;
            Type = (KeymapType)entity.Type;
            Action = (KeymapAction)entity.Action;
        }

        public Shortcut(KeymapKey key, KeymapAction action, KeymapType type = KeymapType.None)
        {
            Key = key;

            Type = type == KeymapType.None ? GetTypeFromAction(action) : type;

            Action = action;
        }

        private KeymapType GetTypeFromAction(KeymapAction action)
        {
            switch (action)
            {
                case KeymapAction.Cockeyed:
                case KeymapAction.Happy:
                case KeymapAction.Sarcastic:
                case KeymapAction.Crying:
                case KeymapAction.Outraged:
                case KeymapAction.Shocked:
                case KeymapAction.Annoyed:
                    return KeymapType.BasicFace;

                case KeymapAction.PickUp:
                case KeymapAction.Sit:
                case KeymapAction.Attack:
                case KeymapAction.Jump:
                    return KeymapType.BasicAction;

                case KeymapAction.EquipmentMenu:
                case KeymapAction.ItemMenu:
                case KeymapAction.AbilityMenu:
                case KeymapAction.SkillMenu:
                case KeymapAction.BuddyList:
                case KeymapAction.WorldMap:
                case KeymapAction.Messenger:
                case KeymapAction.MiniMap:
                case KeymapAction.QuestMenu:
                case KeymapAction.SetKey:
                case KeymapAction.AllChat:
                case KeymapAction.WhisperChat:
                case KeymapAction.PartyChat:
                case KeymapAction.BuddyChat:
                case KeymapAction.Shortcut:
                case KeymapAction.QuickSlot:
                case KeymapAction.ExpandChat:
                case KeymapAction.GuildList:
                case KeymapAction.GuildChat:
                case KeymapAction.PartyList:
                case KeymapAction.QuestHelper:
                case KeymapAction.SpouseChat:
                case KeymapAction.PartySearch:
                case KeymapAction.Medal:
                    return KeymapType.Menu;
            }


            return KeymapType.None;
        }
    }
}
