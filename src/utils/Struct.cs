using System.Collections.Generic;

namespace IonS {

    class StructField {

        public StructField(Word identifier, DataType dataType, int offset) {
            Identifier = identifier;
            DataType = dataType;
            Offset = offset;
        }

        public Word Identifier { get; }
        public DataType DataType { get; }
        public int Offset { get; }

    }

    class Struct {

        public Struct(Word name) {
            Name = name;
            Fields = new Dictionary<string, StructField>();
        }

        public Word Name { get; }
        public Dictionary<string, StructField> Fields { get; }
        private StructField LastField { get; set; }

        public void AddField(StructField field) {
            Fields.Add(field.Identifier.Text, field);
            LastField = field;
        }

        public StructField GetField(string identifier) {
            return Fields.GetValueOrDefault(identifier, null);
        }

        public bool HasField(string identifier) {
            return Fields.ContainsKey(identifier);
        }

        public int GetByteSize() {
            if(LastField == null) return 0;
            return LastField.Offset + EDataType.GetByteSize(LastField.DataType);
        }

    }

}