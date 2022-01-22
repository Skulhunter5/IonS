using System;
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

        public Block(BlockType blockType, Position position) : base(OperationType.Block, position) {
            Id = BlockId();
            BlockType = blockType;
        }
        public int Id { get; }
        public BlockType BlockType { get; }
    }

    sealed class CodeBlock : Block {
        public CodeBlock(Scope parentScope, Procedure procedure, Position position) : base(BlockType.Code, position) {
            Operations = new List<Operation>();
            Scope = new Scope(parentScope, procedure);

            Start = null;
            End = null;
        }

        public List<Operation> Operations { get; set; }
        public Scope Scope { get; }
        public Position Start { get; set; }
        public Position End { get; set; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                foreach(Operation operation in Operations) asm += operation.GenerateAssembly(assembler);
                return asm;
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            foreach(Operation operation in Operations) {
                Error error = operation.TypeCheck(contract);
                if(error != null) return error;
            }
            return null;
        }
    }

    sealed class IfBlock : Block { // TODO: rework
        public IfBlock(CodeBlock blockIf, CodeBlock blockElse, Position position) : base(BlockType.If, position) {
            BlockIf = blockIf;
            BlockElse = blockElse;
            Conditions = new List<CodeBlock>();
            Conditionals = new List<CodeBlock>();
        }

        public CodeBlock BlockIf { get; set; }
        public List<CodeBlock> Conditions { get; }
        public List<CodeBlock> Conditionals { get; }
        public CodeBlock BlockElse { get; set; }

        public override string ToString() {
            List<string> strs = new List<string>();
            if(Conditions.Count > 0) strs.Add("elseifs=" + Conditions.Count);
            if(BlockElse != null) strs.Add("else");
            return "If-statement" + (strs.Count > 0 ? "{" + String.Join(", ", strs) + "}" : "") + " at " + Position;
        }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                asm += "if_" + Id + ":\n";
                asm += "    pop rax\n";
                asm += "    cmp rax, 0\n";
                asm += (Conditionals.Count > 0) ? "    je if_" + Id + "_elseif_" + 0 + "\n" : "    je if_" + Id + "_else\n";
                asm += BlockIf.GenerateAssembly(assembler);
                asm += "    jmp if_" + Id + "_end\n";
                for(int i = 0; i < Conditionals.Count; i++) {
                    asm += "if_" + Id + "_elseif_" + i + ":\n";
                    asm += Conditions[i].GenerateAssembly(assembler);
                    asm += "    pop rax\n";
                    asm += "    cmp rax, 0\n";
                    asm += (i < Conditionals.Count - 1) ? "    je if_" + Id + "_elseif_" + (i+1) + "\n" : "    je if_" + Id + "_else\n";
                    asm += Conditionals[i].GenerateAssembly(assembler);
                    asm += "jmp if_" + Id + "_end\n";
                }
                asm += "if_" + Id + "_else:\n";
                if(BlockElse != null) asm += BlockElse.GenerateAssembly(assembler);
                asm += "if_" + Id + "_end:\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            List<TypeCheckContract> contracts = new List<TypeCheckContract>();

            Error error = contract.Require(DataType.boolean, this);
            if(error != null) return error;

            TypeCheckContract reference = contract.Copy();
            
            error = BlockIf.TypeCheck(contract);
            if(error != null) return error;
            contracts.Add(contract);

            for(int i = 0; i < Conditionals.Count; i++) {
                TypeCheckContract contract1 = reference.Copy();
                Conditions[i].TypeCheck(contract1);

                error = contract1.Require(DataType.boolean, this); // TODO: put something better here (maybe even save the Positions of every elseif and else)
                if(error != null) return error;
                if(!reference.IsCompatible(contract1)) return new SignatureMustBeNoneError(reference, contract1, Conditions[i]);

                error = Conditionals[i].TypeCheck(contract1);
                if(error != null) return error;

                contracts.Add(contract1);
            }

            if(BlockElse != null) {
                TypeCheckContract contract1 = reference.Copy();
                error = BlockElse.TypeCheck(contract1);
                if(error != null) return error;

                contracts.Add(contract1);
            }

            for(int i = 0; i < contracts.Count; i++) if(!contracts[i].IsCompatible(contracts[0])) return new NonMatchingSignaturesError(contracts, this);

            return null;
        }
    }

    sealed class WhileBlock : BreakableBlock {
        public WhileBlock(CodeBlock condition, CodeBlock block, Position position) : base(BlockType.While, position) {
            Condition = condition;
            Block = block;
        }

        public CodeBlock Condition { get; set; }
        public CodeBlock Block { get; set; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                asm += "while_" + Id + ":\n";
                asm += Condition.GenerateAssembly(assembler);
                asm += "    pop rax\n";
                asm += "    cmp rax, 0\n";
                asm += "    je while_" + Id + "_end\n";
                asm += Block.GenerateAssembly(assembler);
                asm += "    jmp while_" + Id + "\n";
                asm += "while_" + Id + "_end:\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override string continue___assembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp while_{Id}
                return "    jmp while_" + Id + "\n";
            }
            throw new NotImplementedException();
        }
        public override string break___assembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp while_{Id}_end
                return "    jmp while_" + Id + "_end\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            Reference = contract.Copy();

            Error error = Condition.TypeCheck(contract);
            if(error != null) return error;

            error = contract.Require(DataType.boolean, this);
            if(error != null) return error;
            if(!contract.IsCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Condition);

            error = Block.TypeCheck(contract);
            if(error != null) return error;
            if(!contract.IsCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Block);

            return null;
        }
    }

    sealed class DoWhileBlock : BreakableBlock {
        public DoWhileBlock(CodeBlock block, CodeBlock condition, Position position) : base(BlockType.DoWhile, position) {
            Block = block;
            Condition = condition;
        }

        public CodeBlock Block { get; set; }
        public CodeBlock Condition { get; set; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                asm += "dowhile_" + Id + ":\n";
                asm += Block.GenerateAssembly(assembler);
                asm += "dowhile_" + Id + "_do:\n";
                asm += Condition.GenerateAssembly(assembler);
                asm += "    pop rax\n";
                asm += "    cmp rax, 0\n";
                asm += "    jne dowhile_" + Id + "\n";
                asm += "dowhile_" + Id + "_end:\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override string continue___assembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp dowhile_{Id}_do
                return "    jmp dowhile_" + Id + "_do\n";
            }
            throw new NotImplementedException();
        }
        public override string break___assembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp dowhile_{Id}_end
                return "    jmp dowhile_" + Id + "_end\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            Reference = contract.Copy();

            Error error = Block.TypeCheck(contract);
            if(error != null) return error;
            if(!contract.IsCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Block);

            error = Condition.TypeCheck(contract);
            if(error != null) return error;

            error = contract.Require(DataType.boolean, this);
            if(error != null) return error;
            if(!contract.IsCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Condition);


            return null;
        }
    }

    abstract class BreakableBlock : Block {
        public BreakableBlock(BlockType blockType, Position position) : base(blockType, position) {}

        public TypeCheckContract Reference { get; set; }

        public abstract string continue___assembly(Assembler assembler);
        public abstract string break___assembly(Assembler assembler);
    }

    sealed class ContinueOperation : Operation {
        public ContinueOperation(BreakableBlock block, Position position) : base(OperationType.Continue, position) {
            Block = block;
        }

        public BreakableBlock Block { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                return Block.continue___assembly(assembler);
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(!contract.IsCompatible(Block.Reference)) return new SignatureMustBeNoneBeforeBreakActionError(Block.Reference, contract, this);

            return null;
        }
    }

    sealed class BreakOperation : Operation {
        public BreakOperation(BreakableBlock block, Position position) : base(OperationType.Break, position) {
            Block = block;
        }

        public BreakableBlock Block { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                return Block.break___assembly(assembler);
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(!contract.IsCompatible(Block.Reference)) return new SignatureMustBeNoneBeforeBreakActionError(Block.Reference, contract, this);

            return null;
        }
    }

}