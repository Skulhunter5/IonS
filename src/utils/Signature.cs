using System.Collections.Generic;
using System;

namespace IonS {

    class Signature {
        private Signature() {
            Types = new DataType[0];
            Size = 0;
        }
        public Signature(DataType[] types) {
            Types = types;
            Size = Types.Length;
        }
        public Signature(List<DataType> types) {
            Types = types.ToArray();
            Size = Types.Length;
        }
        
        public DataType[] Types { get; }
        public int Size { get; }

        public override string ToString() {
            return "(" + GetTypeString() + ")";
        }

        public string GetTypeString() {
            return String.Join(" ", (object[]) Types);
        }

        public bool Equals(Signature other) {
            if(Size != other.Size) return false;
            for(int i = 0; i < Size; i++) if(!Types[i].Equals(other.Types[i])) return false;
            return true;
        }

        // STATIC

        public static readonly Signature I_NONE = new Signature();

    }

}