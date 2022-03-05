using System;

namespace IonS {

    abstract class Variable {

        private static int nextVariableId = 0;
        private static int VariableId() { return nextVariableId++; }

        public Variable(Word identifier) {
            Id = VariableId();
            Identifier = identifier;
        }

        public int Id { get; }
        public Word Identifier { get; }

        public abstract int GetByteSize();
        public abstract DataType GetProvideDataType();

        public string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64) {
                //    var_{Id}: resb {ByteSize}
                return "    var_" + Id + ": resb " + GetByteSize() + "\n";
            }
            if(assembler == Assembler.fasm_linux_x86_64) {
                //    var_{Id}: resb {ByteSize}
                return "    var_" + Id + ": rb " + GetByteSize() + "\n";
            }
            throw new NotImplementedException();
        }

    }

    class DirectVariable : Variable {

        public DirectVariable(Word identifier, int byteSize) : base(identifier) {
            ByteSize = byteSize;
        }

        public int ByteSize { get; }

        public override int GetByteSize() {
            return ByteSize;
        }

        public override DataType GetProvideDataType() {
            return DataType.I_POINTER;
        }

    }

    class DataTypeVariable : Variable {

        public DataTypeVariable(Word identifier, DataType dataType) : base(identifier) {
            DataTypeT = dataType;
        }

        public DataType DataTypeT { get; }

        public override int GetByteSize() {
            return DataTypeT.GetByteSize();
        }

        public override DataType GetProvideDataType() {
            return DataTypeT;
        }

    }

    class StructVariable : Variable {

        public StructVariable(Word identifier, Struct structt) : base(identifier) {
            Structt = structt;
        }

        public Struct Structt { get; }

        public override int GetByteSize() {
            return Structt.GetByteSize();
        }

        public override DataType GetProvideDataType() { // TODO: add support for pointers to structs
            return DataType.I_POINTER;
        }

    }

    class Binding : Variable {
        
        public Binding(Word identifier) : base(identifier) {}

        public DataType DataType { get; set; }
        public int Offset { get; set; }

        public override int GetByteSize() { return 0; }
        public override DataType GetProvideDataType() { return null; }

    }

}