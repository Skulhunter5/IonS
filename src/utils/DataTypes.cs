using System;
using System.Collections.Generic;

namespace IonS {

    class DataType {

        public DataType(uint value) {
            Value = value;
        }
        public DataType(uint value, DataType kind) {
            Value = value;
            Kind = kind;
        }
        public DataType(uint value, Signature signature) {
            Value = value;
            Signature = signature;
        }

        public uint Value { get; }
        public DataType Kind { get; }
        public Signature Signature { get; }

        public override string ToString() {
            if(stringDict.ContainsKey(Value)) return stringDict[Value];
            if(Value == POINTER) {
                if(Kind == null) return "ptr";
                return "ptr<" + Kind + ">";
            }
            throw new NotImplementedException();
        }

        public bool Equals(DataType other) {
            if(Value != other.Value) return false;
            
            if((Kind == null && other.Kind != null) || (Kind != null && other.Kind == null)) return false;
            if(Kind != null) if(!Kind.Equals(other.Kind)) return false;
            
            if((Signature == null && other.Signature != null) || (Signature != null && other.Signature == null)) return false;
            if(Signature != null) if(!Signature.Equals(other.Signature)) return false;

            return true;
        }

        public bool IsType(uint type) {
            return Value == type;
        }

        public uint GetByteSize() {
            if(IsType(DataType.NONE)) throw new NotImplementedException();
            return byteSizeDict[Value];
        }

        public bool IsTypedPointer() {
            return Value == POINTER && Kind != null;
        }

        public bool IsFunction() {
            return Value == FUNCTION;
        }

        // STATIC

        public static readonly uint NONE      = 0x000;
        public static readonly uint BOOLEAN   = 0x001;
        public static readonly uint UINT64    = 0x010;
        public static readonly uint POINTER   = 0x011;
        public static readonly uint FUNCTION  = 0x100;

        public static readonly DataType I_NONE      = new DataType(DataType.NONE);
        public static readonly DataType I_BOOLEAN   = new DataType(DataType.BOOLEAN);
        public static readonly DataType I_UINT64    = new DataType(DataType.UINT64);
        public static readonly DataType I_POINTER   = new DataType(DataType.POINTER);
        public static readonly DataType I_FUNCTION   = new DataType(DataType.FUNCTION);

        public static readonly Dictionary<string, DataType> parseDict = new Dictionary<string, DataType>() {
            {"bool",    I_BOOLEAN},
            {"uint",    I_UINT64},
            {"uint64",  I_UINT64},
            {"ptr",     I_POINTER},
            {"func",    I_FUNCTION},
        };
        public static readonly Dictionary<uint, string> stringDict = new Dictionary<uint, string>() {
            {DataType.NONE,     "none"},
            {DataType.BOOLEAN,  "bool"},
            {DataType.UINT64,   "uint64"},
        };
        public static readonly Dictionary<uint, uint> byteSizeDict = new Dictionary<uint, uint>() {
            {DataType.BOOLEAN,  1},
            {DataType.UINT64,   8},
            {DataType.POINTER,  8},
            {DataType.FUNCTION,  8},
        };

        public static bool IsImplicitlyCastable(DataType from, DataType to) {
            if((from.Value == FUNCTION && to.Value != FUNCTION) || (from.Value != FUNCTION && to.Value == FUNCTION)) return false;
            return true;
        }

        public static bool TryParse(string str, out DataType dataType) {
            if(parseDict.ContainsKey(str)) {
                dataType = parseDict[str];
                return true;
            } else if(str.StartsWith("ptr<") && str.EndsWith('>')) {
                if(!DataType.TryParse(str.Substring(4, str.Length-5), out DataType kind)) {
                    dataType = I_NONE;
                    return false;
                }
                dataType = new DataType(DataType.POINTER, kind);
                return true;
            } else if(str.StartsWith("func<") && str.EndsWith('>')) {
                // CWD
            }
            dataType = I_NONE;
            return false;
        }

    }

}