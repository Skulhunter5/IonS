using System.Collections.Generic;

using System;

namespace IonS {

    class TypeChecker {
        
        private readonly CodeBlock _root;
        private readonly Dictionary<string, Procedure> _procs;

        public TypeChecker(CodeBlock root, Dictionary<string, Procedure> procs) {
            _root = root;
            _procs = procs;
        }

        public Error run() {
            Error error;

            foreach(Procedure procedure in _procs.Values) {
                error = procedure.TypeCheck();
                if(error != null) return error;
            }

            TypeCheckContract contract = new TypeCheckContract();
            error = _root.TypeCheck(contract);
            if(error != null) return error;

            if(!contract.IsEmpty()) Console.WriteLine("[TypeChecker] Warning: excess data on the stack after exit: [" + String.Join(", ", contract.Stack) + "]"); // Error-Warning-System

            return null;
        }

    }

}