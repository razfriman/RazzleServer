using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Constants
{
    public class GameConstants
    {
        //public static readonly SortedDictionary<JobType, bool> CreateJobOptions = new SortedDictionary<JobType, bool>
        //{
        //    {JobType.Resistance,    false},
        //    {JobType.Explorer,      true},
        //    {JobType.Cygnus,        false},
        //    {JobType.Evan,          false},
        //    {JobType.Mercedes,      false},
        //    {JobType.Demon,         false},
        //    {JobType.Phantom,       false},
        //    {JobType.DualBlade,     false},
        //    {JobType.Mihile,        false},
        //    {JobType.Kaiser,        false},
        //    {JobType.AngelicBuster, false},
        //    {JobType.Cannonneer,    false},
        //    {JobType.Xenon,         false},
        //    {JobType.Zero,          false},
        //    {JobType.Shade,         false},
        //    {JobType.Jett,          false},
        //    {JobType.Hayato,        false},
        //    {JobType.Kanna,         false},
        //    {JobType.BeastTamer,    false},
        //};

        public static readonly Dictionary<uint, Tuple<byte, int>> DefaultBasicKeyBinds = new Dictionary<uint, Tuple<byte, int>>
        {
            {2, new Tuple<byte, int>(4, 10)},{3, new Tuple<byte, int>(4, 12)},{4, new Tuple<byte, int>(4, 13)},{5, new Tuple<byte, int>(4, 18)},
            {6, new Tuple<byte, int>(4, 24)},{7, new Tuple<byte, int>(4, 21)},{12, new Tuple<byte, int>(4, 37)},{13, new Tuple<byte, int>(4, 33)},
            {16, new Tuple<byte, int>(4, 8)},{ 17, new Tuple<byte, int>(4, 5)},{ 18, new Tuple<byte, int>(4, 0)},{19, new Tuple<byte, int>(4, 4)},
            { 20, new Tuple<byte, int>(4, 28)},{21, new Tuple<byte, int>(4, 31)},{22, new Tuple<byte, int>(4, 39)},{23, new Tuple<byte, int>(4, 1)},
            { 24, new Tuple<byte, int>(4, 40)},{25, new Tuple<byte, int>(4, 19)},{26, new Tuple<byte, int>(4, 14)},{27, new Tuple<byte, int>(4, 15)},
            { 29, new Tuple<byte, int>(5, 52)},{31, new Tuple<byte, int>(4, 2)},{34, new Tuple<byte, int>(4, 17)},{35, new Tuple<byte, int>(4, 11)},
            { 37, new Tuple<byte, int>(4, 3)},{38, new Tuple<byte, int>(4, 20)},{39, new Tuple<byte, int>(4, 27)},{40, new Tuple<byte, int>(4, 16)},
            { 41, new Tuple<byte, int>(4, 23)},{43, new Tuple<byte, int>(4, 9)},{44, new Tuple<byte, int>(5, 50)},{45, new Tuple<byte, int>(5, 51)},
            { 46, new Tuple<byte, int>(4, 6)},{47, new Tuple<byte, int>(4, 32)},{48, new Tuple<byte, int>(4, 30)},{49, new Tuple<byte, int>(4, 22)},
            { 50, new Tuple<byte, int>(4, 7)},{56, new Tuple<byte, int>(5, 53)},{57, new Tuple<byte, int>(5, 54)},{59, new Tuple<byte, int>(6, 100)},
            { 60, new Tuple<byte, int>(6, 101)},{61, new Tuple<byte, int>(6, 102)},{63, new Tuple<byte, int>(6, 103)},{64, new Tuple<byte, int>(6, 104)},
            { 65, new Tuple<byte, int>(6, 105)},{66, new Tuple<byte, int>(6, 106)}
        };

        public static readonly Dictionary<uint, Tuple<byte, int>> DefaultSecondaryKeyBinds = new Dictionary<uint, Tuple<byte, int>>
        {
            {20, new Tuple<byte, int>(4, 28)},{21, new Tuple<byte, int>(4, 31)},{22, new Tuple<byte, int>(4, 0)},{23, new Tuple<byte, int>(4, 1)},
            { 25, new Tuple<byte, int>(4, 19)},{26, new Tuple<byte, int>(4, 14)},{27, new Tuple<byte, int>(4, 15)},{29, new Tuple<byte, int>(5, 52)},
            { 34, new Tuple<byte, int>(4, 17)},{35, new Tuple<byte, int>(4, 11)},{36, new Tuple<byte, int>(4, 8)},{37, new Tuple<byte, int>(4, 3)},
            { 38, new Tuple<byte, int>(4, 20)},{39, new Tuple<byte, int>(4, 27)},{40, new Tuple<byte, int>(4, 16)},{41, new Tuple<byte, int>(4, 23)},
            { 43, new Tuple<byte, int>(4, 9)},{44, new Tuple<byte, int>(5, 50)},{45, new Tuple<byte, int>(5, 51)},{46, new Tuple<byte, int>(4, 2)},
            { 47, new Tuple<byte, int>(4, 32)},{48, new Tuple<byte, int>(4, 30)},{49, new Tuple<byte, int>(4, 5)},{50, new Tuple<byte, int>(4, 7)},
            { 52, new Tuple<byte, int>(4, 4)},{56, new Tuple<byte, int>(5, 53)},{57, new Tuple<byte, int>(5, 54)},{59, new Tuple<byte, int>(6, 100)},
            { 60, new Tuple<byte, int>(6, 101)},{61, new Tuple<byte, int>(6, 102)},{63, new Tuple<byte, int>(6, 103)},{64, new Tuple<byte, int>(6, 104)},
            { 65, new Tuple<byte, int>(6, 105)},{66, new Tuple<byte, int>(6, 106)},{71, new Tuple<byte, int>(4, 12)},{73, new Tuple<byte, int>(4, 13)},
            { 79, new Tuple<byte, int>(4, 24)},{82, new Tuple<byte, int>(4, 10)},{83, new Tuple<byte, int>(4, 18)}
        };

        public static readonly int[] DefaultBasicQuickSlotKeyMap =
        {
            0x2A, 0x52, 0x47, 0x49,
            0x1D, 0x53, 0x4F, 0x51,
            0x02, 0x03, 0x04, 0x05,
            0x10, 0x11, 0x12, 0x13,
            0x06, 0x07, 0x08, 0x09,
            0x14, 0x1E, 0x1F, 0x20,
            0x0A, 0x0B, 0x21, 0x22
        };

        public static readonly int[] DefaultSecondaryQuickSlotKeyMap =
        {
            0x10, 0x11, 0x12, 0x13,
            0x1E, 0x1F, 0x20, 0x21,
            0x02, 0x03, 0x04, 0x05,
            0x1D, 0x38, 0x2C, 0x2D,
            0x06, 0x07, 0x08, 0x09,
            0x2E, 0x16, 0x17, 0x24,
            0x0A, 0x0B, 0x25, 0x31
        };

        private static readonly int[] PlayerExp = {
            /* 0 ~ 49 */
            1,15,34,57,92,135,372,560,840,1242,1242,1242,1242,1242,1242,1490,1788,2145,2574,3088,3705,4446,5335,6402,7682,9218,11061,13273,15927,
            19112,19112,19112,19112,19112,19112,22934,27520,33024,39628,47553,51357,55465,59902,64694,69869,75458,81494,88013,95054,102658,
            /* 50 ~ 99 */
            110870,119739,129318,139663,150836,162902,175934,190008,205208,221624,221624,221624,221624,221624,221624,239353,258501,
            279181,301515,325636,351686,379820,410205,443021,478462,511954,547790,586135,627164,671065,718039,768301,822082,879627,
            941200,1007084,1077579,1153009,1223719,1320079,1412484,1511357,1617151,1730351,1851475,1981078,2115753,2268135,2426904,2596787,
            /* 100 ~ 149 */
            2596787,2596787,2596787,2596787,2596787,
            2778562,2973061,3181175,3403857,3642126,3897074,4169869,4461759,4774082,5108267,5465845,5848454,6257845,6695894,7164606,7666128,
            8202756,8776948,9391334,10048727,10752137,11504786,12310121,13171829,14093857,15080426,16136055,17265578,18474168,19767359,21151074,
            22631649,24215864,25910974,27724742,29665473,31742056,33963999,36341478,38885381,41607357,44519871,47636261,50970799,54538754,
            /* 150 ~ 199 */
            58356466,62441418,66812317,71489179,76493421,81847960,87577317,93707729,100267270,107285978,113723136,
            120546524,127779315,135446073,143572837,152187207,161318439,170997545,181257397,192132840,203660810,215880458,228833285,242563282,
            257117078,272544102,288896748,306230552,324604385,344080648,364725486,386609015,409805555,434393888,460457521,488084972,517370070,
            548412274,581317010,616196030,653167791,692357858,733899329,777933288,824609285,874085842,926530992,982122851,1041050222,1103513235,
            };

        public static int GetCharacterExpNeeded(int currentLevel)
        {
            return (currentLevel > 249 || currentLevel < 0 ? 0 : PlayerExp[currentLevel]);
        }

        public const int BUFFSTAT_MASKS = 14;
    }
}
