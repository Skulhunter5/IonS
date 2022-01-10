using System.Collections.Generic;

namespace IonS {

    class Scope {

        private static int nextScopeId = 0;
        private static int ScopeId() { return nextScopeId++; }

        public Scope(Scope parent, Procedure procedure) {
            Id = ScopeId();
            Parent = parent;
            Procedure = procedure;
            Variables = new Dictionary<string, Variable>();
        }
        
        public int Id { get; }
        public Scope Parent { get; }
        public Procedure Procedure { get; }
        public Dictionary<string, Variable> Variables { get; }

        public Error RegisterVariable(Variable var) {
            Variable ownVar = GetOwnVariable(var.Identifier.Text);
            if(ownVar != null) return new VariableRedeclarationError(ownVar.Identifier, var.Identifier);
            Variables.Add(Id + "_" + var.Identifier.Text, var);
            return null;
        }
        private Variable GetOwnVariable(string identifier) {
            Variables.TryGetValue(Id + "_" + identifier, out Variable variable);
            return variable;
        }
        public Variable GetVariable(string identifier) {
            Variable ownVar = GetOwnVariable(identifier);
            if(ownVar != null) return ownVar;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }

    }
    
}