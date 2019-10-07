using ProtoBuf;

namespace RazzleServer.DataProvider.References
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LootReference
    {
        public int MobId { get; set; }
        public int ItemId { get; set; }
        public int MinimumQuantity { get; set; } = 1;
        public int MaximumQuantity { get; set; } = 1;
        public int QuestId { get; set; }
        public int Chance { get; set; }
        public bool IsMeso { get; set; }
    }
}
