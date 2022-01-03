namespace IonS {
    
    enum OperationType {
        Push_uint64,
        Add, Subtract, Multiply, Divide, Modulo,
        Dump,
        Drop, Drop2,
        Dup, Dup2,
        Over, Over2, Swap,
        Label, Jump, JumpIfZero, JumpIfNotZero,
        Exit,
        VariableAccess,
        MemRead, MemWrite,

        CodeBlock
    }

    abstract class Operation {
        public Operation(OperationType type) {
            Type = type;
        }
        public OperationType Type { get; }
    }

    // Push operations

    sealed class Push_uint64_Operation : Operation {
        public Push_uint64_Operation(ulong value) : base(OperationType.Push_uint64) {
            Value = value;
        }
        public ulong Value { get; }
    }

    // Calculation operations

    sealed class AddOperation : Operation {
        public AddOperation() : base(OperationType.Add) {}
    }

    sealed class SubtractOperation : Operation {
        public SubtractOperation() : base(OperationType.Subtract) {}
    }

    sealed class MultiplyOperation : Operation {
        public MultiplyOperation() : base(OperationType.Multiply) {}
    }

    sealed class DivideOperation : Operation {
        public DivideOperation() : base(OperationType.Divide) {}
    }

    sealed class ModuloOperation : Operation {
        public ModuloOperation() : base(OperationType.Modulo) {}
    }

    // Dump operation

    sealed class DumpOperation : Operation {
        public DumpOperation() : base(OperationType.Dump) {}
    }

    // Exit operation

    sealed class ExitOperation : Operation {
        public ExitOperation() : base(OperationType.Exit) {}
    }

    // Stack manipulation operations

    sealed class DropOperation : Operation {
        public DropOperation() : base(OperationType.Drop) {}
    }

    sealed class Drop2Operation : Operation {
        public Drop2Operation() : base(OperationType.Drop2) {}
    }

    sealed class DupOperation : Operation {
        public DupOperation() : base(OperationType.Dup) {}
    }

    sealed class Dup2Operation : Operation {
        public Dup2Operation() : base(OperationType.Dup2) {}
    }

    sealed class OverOperation : Operation {
        public OverOperation() : base(OperationType.Over) {}
    }

    sealed class Over2Operation : Operation {
        public Over2Operation() : base(OperationType.Over2) {}
    }

    sealed class SwapOperation : Operation {
        public SwapOperation() : base(OperationType.Swap) {}
    }

    // Label and jump operations

    sealed class LabelOperation : Operation {
        public LabelOperation(string label) : base(OperationType.Label) {
            Label = label;
        }
        public string Label { get; }
    }

    sealed class JumpOperation : Operation {
        public JumpOperation(string label, int direction) : base(OperationType.Jump) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
    }

    sealed class JumpIfZeroOperation : Operation {
        public JumpIfZeroOperation(string label, int direction) : base(OperationType.JumpIfZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
    }

    sealed class JumpIfNotZeroOperation : Operation {
        public JumpIfNotZeroOperation(string label, int direction) : base(OperationType.JumpIfNotZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
    }

    // Variable operations
    
    sealed class VariableAccessOperation : Operation {
        public VariableAccessOperation(string identifier) : base(OperationType.VariableAccess) {
            Identifier = identifier;
        }
        public string Identifier { get; }
    }

    // Mem read/write operations

    sealed class MemReadOperation : Operation {
        public MemReadOperation(byte amount) : base(OperationType.MemRead) {
            Amount = amount;
        }
        public byte Amount { get; }
    }

    sealed class MemWriteOperation : Operation {
        public MemWriteOperation(byte amount) : base(OperationType.MemWrite) {
            Amount = amount;
        }
        public byte Amount { get; }
    }

}