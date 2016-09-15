using System;

namespace RazzleServer.Scripts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Author : Attribute
    {
        public string AuthorName { get; }

        public Author(string name)
        {
            AuthorName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NPC : Attribute
    {
        public string Name { get; }
        private string Description { get; }
        public NPC(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Portal : Attribute
    {
        public string Name { get; }
        private string Description { get; }
        public Portal(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Reactor : Attribute
    {
        public string Name { get; }
        private string Description { get; }
        public Reactor(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Event : Attribute
    {
        public string Name { get; }
        private string Description { get; }
        public Event(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
