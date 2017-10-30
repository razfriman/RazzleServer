using System;
namespace RazzleServer.Common.Exceptions
{
    public class InventoryFullException : Exception
    {
        public override string Message => "The inventory is full.";
    }
}
