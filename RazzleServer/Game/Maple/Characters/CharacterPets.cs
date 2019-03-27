using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterPets : MapleKeyedCollection<int, Pet>
    {
        public Character Parent { get; }

        private short CurrentPetId { get; set; }

        public CharacterPets(Character parent)
        {
            Parent = parent;
        }

        public override int GetKey(Pet item) => item.Item.Id;
        
        public void Save()
        {
        }

//        public Pet LoadPet(Item item)
//        {
//            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM pets WHERE id = " + item.CashId.ToString());
//
//            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
//            if (!data.HasRows)
//            {
//                return null;
//            }
//            else
//            {
//                data.Read(); // Only one pet lol
//                Pet pet = new Pet(item);
//                pet.Name = data.GetString("name");
//                pet.Level = data.GetByte("level");
//                pet.Closeness = data.GetInt16("closeness");
//                pet.Fullness = data.GetByte("fullness");
//                pet.Expiration = data.GetInt64("expiration");
//                if (data.GetInt16("index") == 1)
//                {
//                    CurrentPetId = item.InventorySlot;
//                    pet.Spawned = true;
//                }
//                else
//                {
//                    pet.Spawned = false;
//                }
//
//                mPets.Add(item);
//                return pet;
//            }
//        }

//        public void SpawnPet(Character victim = null)
//        {
//            if (CurrentPetId != 0 && Parent.Inventory.GetItem(5, CurrentPetId) != null)
//            {
//                var item = Parent.Items[ItemType.Cash, CurrentPetId];
//                PetsPacket.SendSpawnPet(Parent, ..Pet, victim);
//            }
//        }

//        public void ChangePetname(string name)
//        {
//            if (CurrentPetId != 0 && Parent.Inventory.GetItem(5, CurrentPetId) != null)
//            {
//                Parent.Inventory.GetItem(5, CurrentPetId).Pet.Name = name;
//                PetsPacket.SendPetNamechange(Parent, name);
//            }
//        }
//
//        public void AddCloseness(short amount)
//        {
//            if (CurrentPetId != 0 && Parent.Inventory.GetItem(5, CurrentPetId) != null)
//            {
//                Pet pet = Parent.Inventory.GetItem(5, CurrentPetId).Pet;
//                if (pet.Closeness + amount > Constants.MaxCloseness)
//                    pet.Closeness = Constants.MaxCloseness;
//                else
//                    pet.Closeness += amount;
//                while (pet.Closeness >= Constants.PetExp[pet.Level - 1] && pet.Level < Constants.PetLevels)
//                {
//                    pet.Level++;
//                    PetsPacket.SendPetLevelup(Parent);
//                }
//            }
//        }
//
        public Pet GetEquippedPet()
        {
            var item = Parent.Items[ItemType.Cash, CurrentPetId];

            if (CurrentPetId != 0 && item?.PetId != null)
            {
                return this[item.PetId.Value];
            }
            
            return null;
        }
    }
}
