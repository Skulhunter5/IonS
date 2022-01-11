using System.Collections.Generic;

using System;

namespace IonS {

    sealed class TypeCheckingResult : Result {
        public TypeCheckingResult(Error error) : base(error) {}
    }

    class TypeChecker {
        
        private readonly CodeBlock _root;
        private readonly Dictionary<string, Procedure> _procs;

        public TypeChecker(CodeBlock root, Dictionary<string, Procedure> procs) {
            _root = root;
            _procs = procs;
        }

        public Error ValidateStacks(Stack<DataType> a, Stack<DataType> b) {
            DataType[] orig = new DataType[a.Count];
            a.CopyTo(orig, 0);
            DataType[] stack = new DataType[b.Count];
            b.CopyTo(stack, 0);
            
            if(stack.Length < orig.Length) {
                List<DataType> missing = new List<DataType>();
                for(int i = orig.Length-1; i >= stack.Length; i--) missing.Add(orig[i]);
                return new MissingDataAfterBlockError(missing.ToArray());
            } else if(stack.Length > orig.Length) {
                List<DataType> excess = new List<DataType>();
                for(int i = orig.Length-1; i >= stack.Length; i--) excess.Add(orig[i]);
                return new ExcessDataAfterBlockError(excess.ToArray());
            }

            return null;
        }

        public Error TypeCheck_CodeBlock(Stack<DataType> stack, CodeBlock block) {
            

            return null;
        }

        public TypeCheckingResult run() {
            Stack<DataType> stack = new Stack<DataType>();

            Stack<DataType> a = new Stack<DataType>();
            Stack<DataType> b = new Stack<DataType>();
            a.Push(DataType.uint64);
            a.Push(DataType.uint8);
            b.Push(DataType.uint64);

            /* Error errort = ValidateStacks(a, b);
            if(errort != null) return new TypeCheckingResult(errort); */

            Error error = TypeCheck_CodeBlock(stack, _root);
            if(error != null) return new TypeCheckingResult(error);

            return new TypeCheckingResult(null);
        }

    }

}