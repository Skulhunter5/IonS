using System.Collections.Generic;

namespace IonS {

    enum BlockType {
        Code,
        If,
        While,
        DoWhile,
    }

    abstract class Block : Operation {
        private static int nextBlockId = 0;
        private static int BlockId() { return nextBlockId++; }

        public Block(BlockType blockType) : base(OperationType.Block) {
            Id = BlockId();
            BlockType = blockType;
        }
        public int Id { get; }
        public BlockType BlockType { get; }
    }

    sealed class CodeBlock : Block {
        public CodeBlock(Scope parentScope, Procedure procedure) : base(BlockType.Code) {
            Operations = new List<Operation>();
            Scope = new Scope(parentScope, procedure);
        }
        public List<Operation> Operations { get; }
        public Scope Scope { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            foreach(Operation operation in Operations) asm += operation.nasm_linux_x86_64();
            return asm;
        }
    }

    sealed class IfBlock : Block { // TODO: rework
        public IfBlock(CodeBlock blockIf, CodeBlock blockElse) : base(BlockType.If) {
            BlockIf = blockIf;
            BlockElse = blockElse;
            Conditions = new List<CodeBlock>();
            Conditionals = new List<CodeBlock>();
        }
        public CodeBlock BlockIf { get; set; }
        public List<CodeBlock> Conditions { get; }
        public List<CodeBlock> Conditionals { get; }
        public CodeBlock BlockElse { get; set; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "if_" + Id + ":\n";
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += (Conditionals.Count > 0) ? "    je if_" + Id + "_elseif_" + 0 + "\n" : "    je if_" + Id + "_else\n";
            asm += BlockIf.nasm_linux_x86_64();
            asm += "    jmp if_" + Id + "_end\n";
            for(int i = 0; i < Conditionals.Count; i++) {
                asm += "if_" + Id + "_elseif_" + i + ":\n";
                asm += Conditions[i].nasm_linux_x86_64();
                asm += "    pop rax\n";
                asm += "    cmp rax, 0\n";
                asm += (i < Conditionals.Count - 1) ? "    je if_" + Id + "_elseif_" + (i+1) + "\n" : "    je if_" + Id + "_else\n";
                asm += Conditionals[i].nasm_linux_x86_64();
                asm += "jmp if_" + Id + "_end\n";
            }
            asm += "if_" + Id + "_else:\n";
            asm += BlockElse != null ? BlockElse.nasm_linux_x86_64() : "";
            asm += "if_" + Id + "_end:\n";
            return asm;
        }
    }

    sealed class WhileBlock : BreakableBlock {
        public WhileBlock(CodeBlock condition, CodeBlock block) : base(BlockType.While) {
            Condition = condition;
            Block = block;
        }
        public CodeBlock Condition { get; set; }
        public CodeBlock Block { get; set; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "while_" + Id + ":\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    je while_" + Id + "_end\n";
            asm += Block.nasm_linux_x86_64();
            asm += "    jmp while_" + Id + "\n";
            asm += "while_" + Id + "_end:\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            //    jmp while_{Id}
            return "    jmp while_" + Id + "\n";
        }
        public override string break___nasm_linux_x86_64() {
            //    jmp while_{Id}_end
            return "    jmp while_" + Id + "_end\n";
        }
    }

    sealed class DoWhileBlock : BreakableBlock {
        public DoWhileBlock(CodeBlock block, CodeBlock condition) : base(BlockType.DoWhile) {
            Block = block;
            Condition = condition;
        }
        public CodeBlock Block { get; set; }
        public CodeBlock Condition { get; set; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "dowhile_" + Id + ":\n";
            asm += Block.nasm_linux_x86_64();
            asm += "dowhile_" + Id + "_do:\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    jne dowhile_" + Id + "\n";
            asm += "dowhile_" + Id + "_end:\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            //    jmp dowhile_{Id}_do
            return "    jmp dowhile_" + Id + "_do\n";
        }
        public override string break___nasm_linux_x86_64() {
            //    jmp dowhile_{Id}_end
            return "    jmp dowhile_" + Id + "_end\n";
        }
    }

    abstract class BreakableBlock : Block {
        public BreakableBlock(BlockType blockType) : base(blockType) {}
        public abstract string continue___nasm_linux_x86_64();
        public abstract string break___nasm_linux_x86_64();
    }

    sealed class ContinueOperation : Operation {
        public ContinueOperation(BreakableBlock block) : base(OperationType.Continue) {
            Block = block;
        }
        public BreakableBlock Block { get; }
        public override string nasm_linux_x86_64() {
            return Block.continue___nasm_linux_x86_64();
        }
    }

    sealed class BreakOperation : Operation {
        public BreakOperation(BreakableBlock block) : base(OperationType.Break) {
            Block = block;
        }
        public BreakableBlock Block { get; }
        public override string nasm_linux_x86_64() {
            return Block.break___nasm_linux_x86_64();
        }
    }

}