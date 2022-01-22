using System.Collections.Generic;

namespace IonS {

    class Optimizer {

        private readonly CodeBlock _root;

        public Optimizer(CodeBlock root) {
            _root = root;
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
            OptimizeBlock(_root);
            return _root;
        }

    }

}