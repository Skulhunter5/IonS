using System;
using System.Collections.Generic;

namespace IonS {

    class TypeCheckContext {

        public TypeCheckContext(Dictionary<string, Dictionary<string, Function>> functions) {
            Functions = functions;
            UsedFunctions = new List<Function>();
        }

        public Dictionary<string, Dictionary<string, Function>> Functions { get; }
        public List<Function> UsedFunctions { get; }

    }

    class TypeCheckContract {
        
        public TypeCheckContract() {
            Stack = new List<DataType>();
        }

        public List<DataType> Stack  { get; }
        public bool HasReturned { get; set; }

        public void SetStackFrom(List<DataType> other) {
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

        public Error RemoveElements(int n, Operation operation) {
            if(Stack.Count < n) return new StackUnderflowError(operation);
            for(int i = 0; i < n; i++) Pop();
            return null;
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
                if(!DataType.IsImplicitlyCastable(Peek(), required[i])) return new UnexpectedDataTypeError(Peek(), required[i], operation);
                if(!Peek().Equals(required[i])) ErrorSystem.AddWarning(new ImplicitCastWarning(Peek(), required[i], operation));
            }
            return null;
        }

        public Error CheckFor(DataType required, Operation operation) {
            if(Stack.Count < 1) return new StackUnderflowError(operation);

            if(!DataType.IsImplicitlyCastable(Peek(), required)) return new UnexpectedDataTypeError(Peek(), required, operation);
            if(!Peek().Equals(required)) ErrorSystem.AddWarning(new ImplicitCastWarning(Peek(), required, operation));

            return null;
        }

        public Error Require(DataType[] required, Operation operation) {
            if(Stack.Count < required.Length) return new StackUnderflowError(operation);

            for(int i = required.Length-1; i >= 0; i--) {
                DataType dataType = Pop();
                if(!DataType.IsImplicitlyCastable(dataType, required[i])) return new UnexpectedDataTypeError(dataType, required[i], operation);
                if(!dataType.Equals(required[i])) ErrorSystem.AddWarning(new ImplicitCastWarning(dataType, required[i], operation));
            }
            return null;
        }

        public Error Require(DataType required, Operation operation) {
            if(Stack.Count < 1) return new StackUnderflowError(operation);

            DataType dataType = Pop();
            if(!DataType.IsImplicitlyCastable(dataType, required)) return new UnexpectedDataTypeError(dataType, required, operation);
            if(!dataType.Equals(required)) ErrorSystem.AddWarning(new ImplicitCastWarning(dataType, required, operation));

            return null;
        }

        public Error Provide(DataType[] provided) {
            for(int i = 0; i < provided.Length; i++) Push(provided[i]);
            return null;
        }

        public Error Provide(DataType provided) {
            Push(provided);
            return null;
        }

        public Error RequireAndProvide(DataType required, DataType provided, Operation operation) {
            Error error = Require(required, operation);
            if(error != null) return error;
            return Provide(provided);
        }

        public Error RequireAndProvide(DataType[] required, DataType provided, Operation operation) {
            Error error = Require(required, operation);
            if(error != null) return error;
            return Provide(provided);
        }

        public Error RequireAndProvide(DataType[] required, DataType[] provided, Operation operation) {
            Error error = Require(required, operation);
            if(error != null) return error;
            return Provide(provided);
        }

        public bool IsStackCompatible(TypeCheckContract other) {
            if(other.Stack.Count != this.Stack.Count) return false;
            for(int i = 0; i < this.Stack.Count; i++) if(!this.Stack[i].Equals(other.Stack[i])) return false;
            return true;
        }
        
        public override int GetHashCode() {
            return HashCode.Combine(Stack.GetHashCode(), HasReturned.GetHashCode());
        }

        public TypeCheckContract Copy() {
            TypeCheckContract clone = new TypeCheckContract();
            clone.SetStackFrom(Stack);
            clone.HasReturned = HasReturned;
            return clone;
        }
        
    }

}