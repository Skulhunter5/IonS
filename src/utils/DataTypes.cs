using System;

namespace IonS {

    enum DataType {
        None,
        boolean,
        uint64,
        pointer
    }

    class EDataType {

        public static bool Is_uint(DataType dataType) {
            return dataType == DataType.uint64;
        }

        public static bool IsImplicitlyCastable(DataType from, DataType to) { // TODO: see how I can improve this (maybe Dictionary and save every possible combination as a string or whatever)
            if(from == to) return true;
            return true;
        }

        public static bool TryParse(string str, out DataType dataType) {
            if(str == "bool") dataType = DataType.boolean;
            else if(str == "uint") dataType = DataType.uint64;
            else if(str == "uint64") dataType = DataType.uint64;
            else if(str == "ptr") dataType = DataType.pointer;
            else {
                dataType = DataType.None;
                return false;
            }
            return true;
        }

        public static string StringOf(DataType dataType) {
            if(dataType == DataType.None) return "NONE";
            if(dataType == DataType.boolean) return "bool";
            if(dataType == DataType.pointer) return "ptr";
            else return ""+dataType;
        }

        public static int GetByteSize(DataType dataType) { // TODO: implement bytesizes
            if(dataType == DataType.None) throw new NotImplementedException();
            if(dataType == DataType.boolean) return 1;
            return 8;
        }

    }

}