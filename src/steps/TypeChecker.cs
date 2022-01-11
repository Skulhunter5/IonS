using System.Collections.Generic;

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

        public TypeCheckingResult run() {
            Stack<DataType> stack = new Stack<DataType>();
            
            return new TypeCheckingResult(null);
        }

    }

}