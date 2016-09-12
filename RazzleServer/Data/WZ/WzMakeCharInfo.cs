using System.Collections.Generic;

namespace RazzleServer.Data.WZ
{
    public class WzMakeCharInfo
    {
        public Dictionary<int, List<int>> MaleValues { get; private set; }
        public Dictionary<int, List<int>> FemaleValues { get; private set; }
        public byte ChoosableGender { get; private set; }
        public WzMakeCharInfo(byte gender, Dictionary<int, List<int>> maleValues, Dictionary<int, List<int>> femaleValues)
        {
            ChoosableGender = gender;
            MaleValues = maleValues;
            FemaleValues = femaleValues;
        }
    }
}
