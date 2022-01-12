using System;
using System.Collections.Generic;

namespace IonS {

    class TypeCheckContract {
        public TypeCheckContract() {
            Stack = new List<DataType>();
        }

        public List<DataType> Stack  { get; }

        private void SetStackFrom(List<DataType> other) {
            Stack.Clear();
            foreach(DataType type in other) Stack.Add(type);
        }

        public void Push(DataType type) {
            Stack.Add(type);
        }
        public DataType Pop() {
            DataType dataType = Stack[Stack.Count-1];
            Stack.RemoveAt(Stack.Count-1);
            return dataType;
        }
        public DataType Peek(int offset=0) {
            return Stack[Stack.Count-1 - offset];
        }
        public bool IsEmpty() {
            return Stack.Count == 0;
        }
        public int GetElementsLeft() {
            return Stack.Count;
        }

        public Error CheckFor(DataType[] required, Operation operation) {
            if(Stack.Count < required.Length) return new StackUnderflowError(operation);

            for(int i = required.Length-1; i >= 0; i--) {
                if(!EDataType.IsImplicitlyCastable(Peek(), required[i])) return new UnexpectedDataTypeError(Peek(), required[i], operation);
                if(Peek() != required[i]) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.String(Peek()) + " to " + required[i] + " during " + operation); // Error-Warning-System
            }
            return null;
        }
        public Error CheckFor(DataType required, Operation operation) {
            if(Stack.Count < 1) return new StackUnderflowError(operation);

            if(!EDataType.IsImplicitlyCastable(Peek(), required)) return new UnexpectedDataTypeError(Peek(), required, operation);
            if(Peek() != required) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.String(Peek()) + " to " + required + " during " + operation); // Error-Warning-System

            return null;
        }
        public Error Require(DataType[] required, Operation operation) {
            if(Stack.Count < required.Length) return new StackUnderflowError(operation);

            for(int i = required.Length-1; i >= 0; i--) {
                DataType dataType = Pop();
                if(!EDataType.IsImplicitlyCastable(dataType, required[i])) {
                    Console.WriteLine("HERE");
                    return new UnexpectedDataTypeError(dataType, required[i], operation);
                }
                if(dataType != required[i]) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.String(dataType) + " to " + required[i] + " during " + operation); // Error-Warning-System
            }
            return null;
        }
        public Error Require(DataType required, Operation operation) {
            if(Stack.Count < 1) return new StackUnderflowError(operation);

            DataType dataType = Pop();
            if(!EDataType.IsImplicitlyCastable(dataType, required)) return new UnexpectedDataTypeError(dataType, required, operation);
            if(dataType != required) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.String(dataType) + " to " + required + " during " + operation); // Error-Warning-System

            return null;
        }
        public Error Provide(DataType[] provided, Operation operation) {
            for(int i = 0; i < provided.Length; i++) Push(provided[i]);
            return null;
        }
        public Error Provide(DataType provided, Operation operation) {
            Push(provided);
            return null;
        }
        public Error RequireAndProvide(DataType[] required, DataType[] provided, Operation operation) {
            Error error = Require(required, operation);
            if(error != null) return error;
            return Provide(provided, operation);
        }
        public Error RequireAndProvide(DataType[] required, DataType provided, Operation operation) {
            Error error = Require(required, operation);
            if(error != null) return error;
            return Provide(provided, operation);
        }

        public TypeCheckContract Copy() {
            TypeCheckContract clone = new TypeCheckContract();
            clone.SetStackFrom(Stack);
            return clone;
        }
    }

}