namespace IonS {

    enum DataType {
        None,
        boolean,
        uint8, uint16, uint32, uint64,
        Pointer
    }

    class EDataType {

        public static bool Is_uint(DataType dataType) {
            return dataType == DataType.uint8 || dataType == DataType.uint16 || dataType == DataType.uint32 || dataType == DataType.uint64;
        }

        public static bool IsImplicitlyCastable(DataType from, DataType to) { // TODO: see how I can improve this (maybe Dictionary and save every possible combination as a string or whatever)
            if(from == to) return true;
            if(to == DataType.boolean) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean;
            else if(to == DataType.uint8) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean;
            else if(to == DataType.uint16) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean;
            else if(to == DataType.uint32) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean;
            else if(to == DataType.uint64) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean || from == DataType.Pointer;
            else if(to == DataType.Pointer) return from == DataType.uint8 || from == DataType.uint16 || from == DataType.uint32 || from == DataType.uint64 || from == DataType.boolean || from == DataType.Pointer;
            return false;
        }

        public static bool TryParse(string str, out DataType dataType) {
            if(str == "bool") dataType = DataType.boolean;
            else if(str == "uint8") dataType = DataType.uint8;
            else if(str == "uint16") dataType = DataType.uint16;
            else if(str == "uint32") dataType = DataType.uint32;
            else if(str == "uint64") dataType = DataType.uint64;
            else if(str == "ptr") dataType = DataType.Pointer;
            else {
                dataType = DataType.None;
                return false;
            }
            return true;
        }

        public static string String(DataType dataType) {
            if(dataType == DataType.None) return "NONE";
            if(dataType == DataType.boolean) return "bool";
            else return ""+dataType;
        }

    }

}