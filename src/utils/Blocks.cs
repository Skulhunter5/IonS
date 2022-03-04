using System;
using System.Collections.Generic;

namespace IonS {

    enum BlockType {
        Code,
        If,
        While,
        DoWhile,
        Switch,

        LetBinding, PeekBinding,
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
        public CodeBlock(Scope scope, Position position) : base(BlockType.Code, position) {
            Operations = new List<Operation>();
            Scope = scope;

            Start = null;
            End = null;
        }

        public List<Operation> Operations { get; set; }
        public Scope Scope { get; }
        public Position Start { get; set; }
        public Position End { get; set; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64 || assembler == Assembler.iasm_linux_x86_64) {
                string asm = "";
                foreach(Operation operation in Operations) asm += operation.GenerateAssembly(assembler);
                return asm;
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            foreach(Operation operation in Operations) {
                Error error = operation.TypeCheck(context, contract);
                if(error != null) return error;
                if(contract.HasReturned) return null;
            }
            return null;
        }
    }

    enum BindingType {
        Let,
        Peek,
    }

    sealed class BindingBlock : Block {
        public BindingBlock(BindingScope scope, CodeBlock code, BindingType bindingType, Position position) : base(BlockType.LetBinding, position) {
            Scope = scope;
            Code = code;
            BindingType = bindingType;
        }

        public BindingScope Scope { get; }
        public CodeBlock Code { get; }
        public BindingType BindingType { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                asm += "    mov rax, [ret_stack_rsp]\n";
                asm += "    sub rax, " + (Scope.BindingsList.Count * 8) + "\n";
                asm += "    mov [ret_stack_rsp], rax\n";
                for(int i = Scope.BindingsList.Count-1; i >= 0; i--) {
                    if(Scope.BindingsList[i] == null) continue;
                    asm += "    mov rbx, [rsp + " + ((Scope.BindingsList.Count-1 - i) * 8) + "]\n";
                    asm += "    mov [rax + " + Scope.BindingsList[i].Offset + "], rbx\n";
                }
                if(BindingType == BindingType.Let) asm += "    add rsp, " + (Scope.BindingsList.Count * 8) + "\n";
                asm += Code.GenerateAssembly(assembler);
                asm += "    mov rax, [ret_stack_rsp]\n";
                asm += "    add rax, " + (Scope.BindingsList.Count * 8) + "\n";
                asm += "    mov [ret_stack_rsp], rax\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override string ToString() {
            return (BindingType == BindingType.Let ? "Let" : "Peek") + "-Binding at " + Position;
        }

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            if(contract.GetElementsLeft() < Scope.BindingsList.Count) return new StackUnderflowError(this);

            for(int i = Scope.BindingsList.Count-1; i >= 0; i--) {
                DataType dataType = BindingType == BindingType.Let ? contract.Pop() : contract.Peek(Scope.BindingsList.Count-1 - i);
                
                if(Scope.BindingsList[i] == null) continue;

                Scope.BindingsList[i].DataType = dataType;
                Scope.BindingsList[i].Offset = i * 8;
            }
            
            return Code.TypeCheck(context, contract);
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

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            List<TypeCheckContract> contracts = new List<TypeCheckContract>();

            Error error = contract.Require(DataType.I_BOOLEAN, this);
            if(error != null) return error;

            TypeCheckContract tmpContract = contract.Copy();
            error = BlockIf.TypeCheck(context, tmpContract);
            if(error != null) return error;
            contracts.Add(tmpContract);

            for(int i = 0; i < Conditionals.Count; i++) {
                tmpContract = contract.Copy();
                Conditions[i].TypeCheck(context, tmpContract);

                error = tmpContract.Require(DataType.I_BOOLEAN, this); // TODO: put something better here (maybe even save the Positions of every elseif and else)
                if(error != null) return error;
                if(!contract.IsStackCompatible(tmpContract)) return new SignatureMustBeNoneError(contract, tmpContract, Conditions[i]);

                error = Conditionals[i].TypeCheck(context, tmpContract);
                if(error != null) return error;

                contracts.Add(tmpContract);
            }

            if(BlockElse != null) {
                tmpContract = contract.Copy();
                error = BlockElse.TypeCheck(context, tmpContract);
                if(error != null) return error;

                contracts.Add(tmpContract);
            }

            bool HasReturned = true;
            foreach(TypeCheckContract cntr in contracts) if(!cntr.HasReturned) {
                HasReturned = false;
                break;
            }
            if(BlockElse != null) contract.HasReturned = HasReturned;

            if(!HasReturned) {
                TypeCheckContract reference = null;
                int j = 0;
                for(int i = 0; i < contracts.Count; i++) if(!contracts[i].HasReturned) {
                    reference = contracts[i];
                    j = i;
                    break;
                }
                for(int i = j+1; i < contracts.Count; i++) if(!contracts[i].HasReturned) if(!contracts[i].IsStackCompatible(contracts[j])) return new NonMatchingSignaturesError(contracts, this);
                if(BlockElse == null) {
                    if(!contract.IsStackCompatible(contracts[0])) return new SignatureMustBeNoneError(contract, contracts[0], BlockIf);
                } else contract.SetStackFrom(reference.Stack);
            }

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

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            Reference = contract.Copy();

            Error error = Condition.TypeCheck(context, contract);
            if(error != null) return error;

            error = contract.Require(DataType.I_BOOLEAN, this);
            if(error != null) return error;
            if(!contract.IsStackCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Condition);

            error = Block.TypeCheck(context, contract);
            if(error != null) return error;
            if(!contract.IsStackCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Block);

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

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            Reference = contract.Copy();

            Error error = Block.TypeCheck(context, contract);
            if(error != null) return error;
            if(!contract.IsStackCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Block);

            error = Condition.TypeCheck(context, contract);
            if(error != null) return error;

            error = contract.Require(DataType.I_BOOLEAN, this);
            if(error != null) return error;
            if(!contract.IsStackCompatible(Reference)) return new SignatureMustBeNoneError(Reference, contract, Condition);


            return null;
        }
    }

    sealed class SwitchBlock : BreakableBlock {
        public SwitchBlock(Position position) : base(BlockType.Switch, position) {
            Cases = new List<CodeBlock>();
            Blocks = new List<CodeBlock>();
        }

        public List<CodeBlock> Cases { get; }
        public List<CodeBlock> Blocks { get; }
        public CodeBlock DefaultBlock { get; set; }

        public override string ToString() {
            return "Switch-statement{" + Cases.Count + " cases} at " + Position;
        }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";

                asm += "switch_" + Id + ":\n";

                for(int i = 0; i < Cases.Count; i++) {
                    asm += Cases[i].GenerateAssembly(assembler);
                    asm += "    pop rax\n";
                    asm += "    cmp rax, 0\n";
                    asm += "    jne switch_" + Id + "_case_" + i + "\n";
                }

                asm += "    jmp switch_" + Id + "_" + (DefaultBlock != null ? "default" : "end") + "\n";

                for(int i = 0; i < Cases.Count; i++) {
                    asm += "switch_" + Id + "_case_" + i + ":\n";
                    asm += Blocks[i].GenerateAssembly(assembler);
                }

                if(DefaultBlock != null) {
                    asm += "switch_" + Id + "_default:\n";
                    asm += DefaultBlock.GenerateAssembly(assembler);
                }

                asm += "switch_" + Id + "_end:\n";

                return asm;
            }
            throw new NotImplementedException();
        }

        public override string continue___assembly(Assembler assembler) {
            throw new NotImplementedException(); // Unreachable
        }

        public override string break___assembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp switch_{Id}_end
                return "    jmp switch_" + Id + "_end\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            Reference = contract.Copy();

            List<TypeCheckContract> contracts = new List<TypeCheckContract>();

            for(int i = 0; i < Cases.Count; i++) {
                TypeCheckContract contract1 = Reference.Copy();
                Cases[i].TypeCheck(context, contract1);

                Error error = contract1.Require(DataType.I_BOOLEAN, this);
                if(error != null) return error;
                if(!Reference.IsStackCompatible(contract1)) return new SignatureMustBeNoneError(Reference, contract1, Cases[i]);

                error = Blocks[i].TypeCheck(context, contract1);
                if(error != null) return error;

                contracts.Add(contract1);
            }

            if(DefaultBlock != null) {
                TypeCheckContract contract1 = Reference.Copy();
                Error error = DefaultBlock.TypeCheck(context, contract1);
                if(error != null) return error;

                contracts.Add(contract1);
            }

            bool HasReturned = true;
            foreach(TypeCheckContract cntr in contracts) if(!cntr.HasReturned) {
                HasReturned = false;
                break;
            }
            contract.HasReturned = HasReturned;

            if(!HasReturned) for(int i = 0; i < contracts.Count; i++) if(!contracts[i].HasReturned) if(!contracts[i].IsStackCompatible(contracts[0])) return new NonMatchingSignaturesError(contracts, this);

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

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            if(!contract.IsStackCompatible(Block.Reference)) return new SignatureMustBeNoneBeforeBreakActionError(Block.Reference, contract, this);

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

        public override Error TypeCheck(TypeCheckContext context, TypeCheckContract contract) {
            if(!contract.IsStackCompatible(Block.Reference)) return new SignatureMustBeNoneBeforeBreakActionError(Block.Reference, contract, this);

            return null;
        }
    }

}