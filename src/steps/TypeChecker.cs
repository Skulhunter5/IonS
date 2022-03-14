using System.Collections.Generic;

using System;

namespace IonS {

    class TypeChecker {
        
        private readonly CodeBlock _root;
        private readonly Dictionary<string, Dictionary<string, Function>> _functions;
        private readonly TypeCheckContext _ctx;

        public TypeChecker(CodeBlock root, Dictionary<string, Dictionary<string, Function>> functions) {
            _root = root;
            _functions = functions;
            _ctx = new TypeCheckContext(functions);
        }

        public Error run() {
            Error error;

            foreach(var overloads in _functions.Values) {
                foreach(Function functions in overloads.Values) {
                    error = functions.TypeCheck(_ctx);
                    if(error != null) return error;
                }
            }

            TypeCheckContract contract = new TypeCheckContract();
            error = _root.TypeCheck(_ctx, contract);
            if(error != null) return error;

            // TODO: fix probably too many lines
            foreach(Function function in _ctx.UsedFunctions) function.Use();

            if(!contract.IsEmpty()) Console.WriteLine("[TypeChecker] Warning: excess data on the stack after exit: [" + String.Join(", ", contract.Stack) + "]"); // Error-Warning-System
            
            return null;
        }

    }

}