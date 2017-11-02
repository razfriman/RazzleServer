using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedItems : KeyedCollection<int, Item>
    {
        public List<int> WizetItemIDs { get; private set; }

        private readonly ILogger Log = LogManager.Log;

        public CachedItems()
        {
            Log.LogInformation("Loading Items");
            foreach (Datum datum in new Datums("item_data").Populate())
            {
                Add(new Item(datum));
            }

            Log.LogInformation("Loading Consumables");
            foreach (Datum datum in new Datums("item_consume_data").Populate())
            {
                this[(int)datum["itemid"]].LoadConsumeData(datum);
            }

            Log.LogInformation("Loading Equips");
            foreach (Datum datum in new Datums("item_equip_data").Populate())
            {
                this[(int)datum["itemid"]].LoadEquipmentData(datum);
            }

            Log.LogInformation("Loading Summons");
            foreach (Datum datum in new Datums("item_summons").Populate())
            {
                this[(int)datum["itemid"]].Summons.Add(new Tuple<int, short>((int)datum["mobid"], (short)datum["chance"]));
            }

            WizetItemIDs = new List<int>(4)
            {
                1002140,
                1322013,
                1042003,
                1062007
            };
        }

        protected override int GetKeyForItem(Item item) => item.MapleID;
    }
}