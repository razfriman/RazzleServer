using System;
using System.Runtime.Serialization;

namespace RazzleServer.Common.Wz.WzStructure
{
    [DataContract]
    public struct MapleBool //I know I could have used the nullable bool.
    {
        public const byte NotExist = 0;
        public const byte False = 1;
        public const byte True = 2;

        [DataMember]
        private byte val { get; set; }
        public static implicit operator MapleBool(byte value)
        {
            return new MapleBool
            {
                val = value
            };
        }

        public static implicit operator MapleBool(bool? value)
        {
            return new MapleBool
            {
                val = value == null ? NotExist : (bool)value ? True : False
            };
        }

        public static implicit operator bool(MapleBool value)
        {
            return value == True;
        }

        public static implicit operator byte(MapleBool value)
        {
            return value.val;
        }

        public override bool Equals(object obj)
        {
            return obj is MapleBool && ((MapleBool)obj).val.Equals(val);
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public static bool operator ==(MapleBool a, MapleBool b)
        {
            return a.val.Equals(b.val);
        }

        public static bool operator ==(MapleBool a, bool b)
        {
            return b && a.val == True || !b && a.val == False;
        }

        public static bool operator !=(MapleBool a, MapleBool b)
        {
            return !a.val.Equals(b.val);
        }

        public static bool operator !=(MapleBool a, bool b)
        {
            return b && a.val != True || !b && a.val != False;
        }

        public bool HasValue => val != NotExist;

        public bool Value
        {
            get
            {
                switch (val)
                {
                    case False:
                        return false;
                    case True:
                        return true;
                    default:
                        throw new Exception("Tried to get value of nonexistant MapleBool");
                }
            }
        }
    }
}
