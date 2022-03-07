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

        public uint Value { get; }
        public DataType Kind { get; }

        public override string ToString() {
            if(stringDict.ContainsKey(Value)) return stringDict[Value];
            if(Value == POINTER) {
                if(Kind == null) return "ptr";
                return "ptr<" + Kind + ">";
            }
            throw new NotImplementedException();
        }

        public bool Equals(DataType dataType) {
            if(Value != dataType.Value) return false;
            if(Kind != dataType.Kind) return false;
            if(Kind != null) if(!Kind.Equals(dataType.Kind)) return false;
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

        // STATIC

        public static readonly uint NONE     = 0x00;
        public static readonly uint BOOLEAN  = 0x01;
        public static readonly uint UINT64   = 0x10;
        public static readonly uint POINTER  = 0x11;

        public static readonly DataType I_NONE      = new DataType(DataType.NONE);
        public static readonly DataType I_BOOLEAN   = new DataType(DataType.BOOLEAN);
        public static readonly DataType I_UINT64    = new DataType(DataType.UINT64);
        public static readonly DataType I_POINTER   = new DataType(DataType.POINTER);

        public static readonly Dictionary<string, DataType> parseDict = new Dictionary<string, DataType>() {
            {"bool",    I_BOOLEAN},
            {"uint",    I_UINT64},
            {"uint64",  I_UINT64},
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
        };

        public static bool IsImplicitlyCastable(DataType from, DataType to) { // TODO: see how I can improve this (maybe Dictionary and save every possible combination as a string or whatever)
            if(from == to) return true;
            return true;
        }

        public static bool TryParse(string str, out DataType dataType) {
            if(parseDict.ContainsKey(str)) {
                dataType = parseDict[str];
                return true;
            } else if(str == "ptr") {
                    dataType = I_POINTER;
                    return true;
            } else if(str.StartsWith("ptr<") && str.EndsWith('>')) {
                if(!DataType.TryParse(str.Substring(4, str.Length-5), out DataType kind)) {
                    dataType = I_NONE;
                    return false;
                }
                dataType = new DataType(DataType.POINTER, kind);
                return true;
            }
            dataType = I_NONE;
            return false;
        }

    }

}