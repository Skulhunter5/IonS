using System;
using System.Collections.Generic;

namespace IonS {

    class DataType {

        public DataType(int value) {
            Value = value;
        }

        public int Value { get; }

        public override string ToString() {
            return stringDict[Value];
        }

        public bool Equals(DataType dataType) {
            if(Value != dataType.Value) return false;
            return true;
        }

        public bool IsType(int type) {
            return Value == type;
        }

        public int GetByteSize() {
            if(IsType(DataType.NONE)) throw new NotImplementedException();
            return bytesizeDict[Value];
        }

        public static readonly int NONE     = 0x00;
        public static readonly int BOOLEAN  = 0x01;
        public static readonly int UINT64   = 0x10;
        public static readonly int POINTER  = 0x11;

        public static readonly DataType I_NONE      = new DataType(DataType.NONE);
        public static readonly DataType I_BOOLEAN   = new DataType(DataType.BOOLEAN);
        public static readonly DataType I_UINT64    = new DataType(DataType.UINT64);
        public static readonly DataType I_POINTER   = new DataType(DataType.POINTER);

        public static readonly Dictionary<string, DataType> parseDict = new Dictionary<string, DataType>() {
            {"bool",    I_BOOLEAN},
            {"uint",    I_UINT64},
            {"uint64",  I_UINT64},
            {"ptr",     I_POINTER},
        };
        public static readonly Dictionary<int, string> stringDict = new Dictionary<int, string>() {
            {DataType.NONE,     "none"},
            {DataType.BOOLEAN,  "bool"},
            {DataType.UINT64,   "uint64"},
            {DataType.POINTER,  "ptr"},
        };
        public static readonly Dictionary<int, int> bytesizeDict = new Dictionary<int, int>() {
            {DataType.BOOLEAN,  1},
            {DataType.UINT64,   8},
            {DataType.POINTER,  8},
        };

        public static bool Is_uint(DataType dataType) {
            return dataType.IsType(DataType.UINT64);
        }

        public static bool IsImplicitlyCastable(DataType from, DataType to) { // TODO: see how I can improve this (maybe Dictionary and save every possible combination as a string or whatever)
            if(from == to) return true;
            return true;
        }

        public static bool TryParse(string str, out DataType dataType) {
            if(parseDict.ContainsKey(str)) {
                dataType = parseDict[str];
                return true;
            } else {
                dataType = I_NONE;
                return false;
            }
        }

    }

}