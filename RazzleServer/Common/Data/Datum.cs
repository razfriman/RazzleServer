//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace RazzleServer.Common.Data
//{
//    public sealed class Datums : IEnumerable<Datum>
//    {
//        private string Table { get; set; }
//        private List<Datum> Values { get; set; }
//        private string ConnectionString { get; set; }

//        public Datums(string table)
//        {
//            Table = table;
//        }

//        public Datums(string table, string schema)
//        {

//        }

//        private void PopulateInternal(string fields, string constraints, params object[] args)
//        {

//        }

//        public Datums Populate()
//        {
//            PopulateInternal(null, null, null);

//            return this;
//        }

//        public Datums Populate(string constraints, params object[] args)
//        {
//            PopulateInternal(null, constraints, args);

//            return this;
//        }

//        public Datums PopulateWith(string fields)
//        {
//            PopulateInternal(fields, null, null);

//            return this;
//        }

//        public Datums PopulateWith(string fields, string constraints, params object[] args)
//        {
//            PopulateInternal(fields, constraints, args);

//            return this;
//        }

//        public IEnumerator<Datum> GetEnumerator()
//        {
//            foreach (Datum loopDatum in Array.Empty<Datum>())
//            {
//                yield return loopDatum;
//            }
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return (IEnumerator)GetEnumerator();
//        }
//    }

//    public sealed class Datum
//    {
//        public string Table { get; private set; }
//        public Dictionary<string, Object> Dictionary { get; set; }
//        private string ConnectionString { get; set; }

//        public object this[string name]
//        {
//            get
//            {
//                return 0;
//            }
//            set
//            {

//            }
//        }

//        public Datum(string table)
//        {
//            Table = table;
//            Dictionary = new Dictionary<string, object>();
//        }

//        public Datum(string table, string schema)
//        {
//            Table = table;
//            Dictionary = new Dictionary<string, object>();
//        }

//        public Datum(string table, Dictionary<string, object> dictionary)
//        {
//            Table = table;
//            Dictionary = dictionary;
//        }

//        public Datum(string table, string schema, Dictionary<string, object> dictionary)
//        {
//            Table = table;
//            Dictionary = dictionary;
//        }

//        public Datum Populate(string constraints, params object[] args)
//        {
//            PopulateWith("*", constraints, args);

//            return this;
//        }

//        public Datum PopulateWith(string fields, string constraints, params object[] args)
//        {
//            return this;
//        }

//        public void Insert()
//        {
//        }

//        public int InsertAndReturnId()
//        {
//            return 0;
//        }

//        public void Update(string constraints, params object[] args)
//        {
//        }
//    }
//}