using System.Collections.Generic;

namespace IonS {

    class Scope {

        private static int nextScopeId = 0; // FIXME: remove
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

        public virtual bool RegisterVariable(Variable var) {
            Variables.TryGetValue(var.Identifier.Text, out Variable ownVar);
            if(ownVar != null) {
                ErrorSystem.AddError_s(new VariableRedeclarationError(ownVar.Identifier, var.Identifier));
                return false;
            }
            Variables.Add(var.Identifier.Text, var);
            return true;
        }

        public virtual Variable GetVariable(string identifier) {
            Variables.TryGetValue(identifier, out Variable ownVar);
            if(ownVar != null) return ownVar;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }

        public virtual int GetRetStackOffset() {
            if(Parent != null) return Parent.GetRetStackOffset();
            return 0;
        }

        public virtual int GetBindingOffset(Binding binding) {
            return Parent.GetBindingOffset(binding);
        }

    }

    class BindingScope : Scope {

        public BindingScope(Scope parent, Procedure procedure, List<Binding> bindingsList) : base(parent, procedure) {
            Bindings = new Dictionary<string, Binding>();
            BindingsList = bindingsList;
            for(int i = 0; i < bindingsList.Count; i++) if(bindingsList[i] != null) Bindings.Add(bindingsList[i].Identifier.Text, bindingsList[i]);
        }

        public Dictionary<string, Binding> Bindings { get; }
        public List<Binding> BindingsList { get; }

        public override bool RegisterVariable(Variable var) {
            Variables.TryGetValue(var.Identifier.Text, out Variable ownVar);
            if(ownVar != null) {
                ErrorSystem.AddError_s(new VariableRedeclarationError(ownVar.Identifier, var.Identifier));
                return false;
            }
            Bindings.TryGetValue(var.Identifier.Text, out Binding binding);
            if(binding != null) {
                ErrorSystem.AddError_s(new VariableRedeclarationError(ownVar.Identifier, var.Identifier));
                return false;
            }
            Variables.Add(var.Identifier.Text, var);
            return true;
        }

        public override Variable GetVariable(string identifier) {
            Variables.TryGetValue(identifier, out Variable var);
            if(var != null) return var;
            Bindings.TryGetValue(identifier, out Binding binding);
            if(binding != null) return binding;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }

        public override int GetRetStackOffset() {
            int offset = BindingsList.Count * 8;
            if(Parent != null) offset += Parent.GetRetStackOffset();
            return offset;
        }

        public override int GetBindingOffset(Binding binding) {
            if(BindingsList.Contains(binding)) return 0;
            return Parent.GetBindingOffset(binding) + BindingsList.Count * 8;
        }

    }
    
}