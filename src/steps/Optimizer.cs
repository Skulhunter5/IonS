using System.Collections.Generic;

namespace IonS {

    class Optimizer {

        private readonly CodeBlock _root;
        private readonly Dictionary<string, Dictionary<string, Function>> _functions;

        public Optimizer(CodeBlock root, Dictionary<string, Dictionary<string, Function>> functions) {
            _root = root;
            _functions = functions;
        }

        private void OptimizeBlock(CodeBlock block) {
            List<Operation> operations = new List<Operation>();
            for(int i = 0; i < block.Operations.Count; i++) {
                if(block.Operations[i].Type == OperationType.Drop) {
                    int n = ((DropOperation) block.Operations[i]).N;
                    while(i < block.Operations.Count-1 && block.Operations[i+1].Type == OperationType.Drop) n += ((DropOperation) block.Operations[++i]).N;
                    operations.Add(new DropOperation(n, null));
                } else operations.Add(block.Operations[i]);
            }
            block.Operations = operations;
        }

        public CodeBlock run() {
            // TODO: optimize Inline Functions even more by inlining them before optimization so it can optimize them where they are (maybe even: optimize inline functions, inline them, optimize code)
            foreach(Dictionary<string, Function> functions in _functions.Values) foreach(Function function in functions.Values) OptimizeBlock(function.Body);
            OptimizeBlock(_root);
            return _root;
        }

    }

}