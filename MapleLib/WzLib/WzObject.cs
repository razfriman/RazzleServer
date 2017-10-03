using System;
using System.Drawing;

namespace MapleLib.WzLib
{
	/// <summary>
	/// An abstract class for wz objects
	/// </summary>
	public abstract class WzObject : IDisposable
	{
        private object tag;
        private object tag2;
        private object tag3;

		public abstract void Dispose();

		/// <summary>
		/// The name of the object
		/// </summary>
		public abstract string Name { get; set; }
		/// <summary>
		/// The WzObjectType of the object
		/// </summary>
		public abstract WzObjectType ObjectType { get; }
		/// <summary>
		/// Returns the parent object
		/// </summary>
		public abstract WzObject Parent { get; internal set; }
        /// <summary>
        /// Returns the parent WZ File
        /// </summary>
        public abstract WzFile WzFileParent { get; }

        public WzObject this[string name]
        {
            get
            {
                if (this is WzFile)
                {
                    return ((WzFile)this)[name];
                }
                if (this is WzDirectory)
                {
                    return ((WzDirectory)this)[name];
                }
                if (this is WzImage)
                {
                    return ((WzImage)this)[name];
                }
                if (this is WzImageProperty)
                {
                    return ((WzImageProperty)this)[name];
                }
                throw new NotImplementedException();
            }
        }

        public string FullPath
        {
            get
            {
                if (this is WzFile) return ((WzFile)this).WzDirectory.Name;
                string result = this.Name;
                WzObject currObj = this;
                while (currObj.Parent != null)
                {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }
                return result;
            }
        }

        /// <summary>
        /// Used in HaCreator to save already parsed images
        /// </summary>
        public virtual object HCTag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>
        /// Used in HaCreator's MapSimulator to save already parsed textures
        /// </summary>
        public virtual object MSTag
        {
            get { return tag2; }
            set { tag2 = value; }
        }

        /// <summary>
        /// Used in HaRepacker to save WzNodes
        /// </summary>
        public virtual object HRTag
        {
            get { return tag3; }
            set { tag3 = value; }
        }

        public virtual object WzValue { get { return null; } }

        public abstract void Remove();

        //Credits to BluePoop for the idea of using cast overriding
        //2015 - That is the worst idea ever, removed and replaced with Get* methods
        #region Cast Values
        public virtual int GetInt()
        {
            throw new NotImplementedException();
        }

        public virtual short GetShort()
        {
            throw new NotImplementedException();
        }

        public virtual long GetLong()
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat()
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual Point GetPoint()
        {
            throw new NotImplementedException();
        }

        public virtual Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
        #endregion

	}
}