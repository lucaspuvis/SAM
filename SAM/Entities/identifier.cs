using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.PlaceHolders
{
    public abstract class Identifier : Interfaces.IIdentifier
    {
        int id;
        string original;

        public Identifier(int id, string original) { this.id = id; this.original = original; }

        public int getId()
        {
            return id;
        }

        public string getOriginal()
        {
            return original;
        }

        override
        public string ToString()
            {
            return original;
        }
    }
    // Comment 
    public class CUID : Identifier
    {
        public CUID(int id, string original) : base(id, original) { } 
    }
    // Sentence
    public class SUID : Identifier
    {
        public SUID(int id, string original) : base(id, original) { }
    }
    // TestData
    public class TUID : Identifier
    {
        public TUID(int id, string original) : base(id, original) { }
    }



}
