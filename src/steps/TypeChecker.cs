using System.Collections.Generic;

using System;

namespace IonS {

    class TypeChecker {
        
        private readonly CodeBlock _root;
        private readonly Dictionary<string, Dictionary<string, Procedure>> _procs;
        private readonly TypeCheckContext _ctx;

        public TypeChecker(CodeBlock root, Dictionary<string, Dictionary<string, Procedure>> procs) {
            _root = root;
            _procs = procs;
            _ctx = new TypeCheckContext(procs);
        }

        public Error run() {
            Error error;

            foreach(var overloads in _procs.Values) {
                foreach(Procedure procedure in overloads.Values) {
                    error = procedure.TypeCheck(_ctx);
                    if(error != null) return error;
                }
            }

            TypeCheckContract contract = new TypeCheckContract();
            error = _root.TypeCheck(_ctx, contract);
            if(error != null) return error;

            // TODO: fix probably too many lines
            foreach(Procedure procedure in _ctx.UsedProcedures) procedure.Use();

            if(!contract.IsEmpty()) Console.WriteLine("[TypeChecker] Warning: excess data on the stack after exit: [" + String.Join(", ", contract.Stack) + "]"); // Error-Warning-System
            
            return null;
        }

    }

}