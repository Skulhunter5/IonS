using System;
using System.Collections.Generic;

namespace IonS {

    class SimulationResult {
        public SimulationResult(byte exitcode, Error error) {
            Exitcode = exitcode;
            Error = error;
        }
        public byte Exitcode { get; }
        public Error Error { get; }
    }

    class Simulator { // TODO: Implement variables
        public readonly Stack<ulong> stack;
        public readonly int maxStackSize;

        public Simulator(int maxStackSize) {
            stack = new Stack<ulong>();
            this.maxStackSize = maxStackSize;
        }

        public SimulationResult run(string text, string source) {
            var parser = new Parser(text, source);
            var result = parser.Parse();
            if(result.Error != null) return new SimulationResult(0, result.Error);
            var operations = result.Operations;
            for(int i = 0; i < operations.Count; i++) {
                Operation operation = operations[i];
                switch(operation.Type) {
                    case OperationType.Exit: {
                        if(stack.Count == 0) return new SimulationResult(0, new StackUnderflowError("exit"));

                        return new SimulationResult((byte) stack.Pop(), null);
                    }
                    case OperationType.Push_uint64: {
                        Push_uint64_Operation pushOperation = (Push_uint64_Operation) operation;
                        if(stack.Count >= maxStackSize) return new SimulationResult(0, new StackUnderflowError(""+pushOperation.Value));

                        stack.Push(pushOperation.Value);
                        break;
                    }
                    case OperationType.Drop: {
                        if(stack.Count == 0) return new SimulationResult(0, new StackUnderflowError("drop"));

                        stack.Pop();
                        break;
                    }
                    case OperationType.Drop2: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("2drop"));

                        stack.Pop();
                        stack.Pop();
                        break;
                    }
                    case OperationType.Dup: {
                        if(stack.Count == 0) return new SimulationResult(0, new StackUnderflowError("dup"));
                        if(stack.Count >= maxStackSize) return new SimulationResult(0, new StackOverflowError("dup"));

                        ulong a = stack.Pop();
                        stack.Push(a);
                        stack.Push(a);
                        break;
                    }
                    case OperationType.Dup2: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("2dup"));
                        if(stack.Count >= maxStackSize - 1) return new SimulationResult(0, new StackOverflowError("2dup"));

                        ulong b = stack.Pop();
                        ulong a = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    }
                    case OperationType.Swap: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("swap"));

                        ulong a = stack.Pop();
                        ulong b = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    }
                    case OperationType.Over: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("over"));
                        if(stack.Count >= maxStackSize) return new SimulationResult(0,  new StackOverflowError("over"));

                        ulong b = stack.Pop();
                        ulong a = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(a);
                        break;
                    }
                    case OperationType.Over2: {
                        if(stack.Count <= 2) return new SimulationResult(0, new StackUnderflowError("2over"));
                        if(stack.Count >= maxStackSize - 1) return new SimulationResult(0, new StackOverflowError("2over"));

                        ulong d = stack.Pop();
                        ulong c = stack.Pop();
                        ulong b = stack.Pop();
                        ulong a = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(c);
                        stack.Push(d);
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    }
                    case OperationType.Add: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("+"));

                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    }
                    case OperationType.Subtract: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("-"));

                        ulong b = stack.Pop();
                        ulong a = stack.Pop();
                        stack.Push(a - b);
                        break;
                    }
                    case OperationType.Multiply: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("*"));

                        stack.Push(stack.Pop() * stack.Pop());
                        break;
                    }
                    case OperationType.Divide: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("/"));

                        ulong b = stack.Pop();
                        if(b == 0) return new SimulationResult(0, new DivideByZeroError("/"));

                        ulong a = stack.Pop();
                        stack.Push(a / b);
                        break;
                    }
                    case OperationType.Modulo: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("%"));

                        ulong b = stack.Pop();
                        if(b == 0) return new SimulationResult(0, new DivideByZeroError("%"));

                        ulong a = stack.Pop();
                        stack.Push(a % b);
                        break;
                    }
                    case OperationType.Dump: {
                        if(stack.Count == 0) return new SimulationResult(0, new StackUnderflowError("."));

                        Console.WriteLine(stack.Pop());
                        break;
                    }
                    case OperationType.Jump: {
                        JumpOperation jumpOperation = (JumpOperation) operation;
                        while(i >= 0 && i < operations.Count && (operations[i].Type != OperationType.Label || ((LabelOperation) operations[i]).Label != jumpOperation.Label))
                            i += jumpOperation.Direction;
                        break;
                    }
                    case OperationType.JumpIfZero: {
                        if(stack.Pop() == 0) {
                            JumpIfZeroOperation jumpIfZeroOperation = ((JumpIfZeroOperation) operation);
                            while(i >= 0 && i < operations.Count && (operations[i].Type != OperationType.Label || ((LabelOperation) operations[i]).Label != jumpIfZeroOperation.Label))
                                i += jumpIfZeroOperation.Direction;
                        }
                        break;
                    }
                    case OperationType.JumpIfNotZero: {
                        if(stack.Pop() != 0) {
                            JumpIfNotZeroOperation jumpIfNotZeroOperation = ((JumpIfNotZeroOperation) operation);
                            while(i >= 0 && i < operations.Count && (operations[i].Type != OperationType.Label || ((LabelOperation) operations[i]).Label != jumpIfNotZeroOperation.Label))
                                i += jumpIfNotZeroOperation.Direction;
                        }
                        break;
                    }
                    case OperationType.Label: {
                        break;
                    }
                    default: {
                        return new SimulationResult(0, new UnimplementedOperationSimulatorError(operation.Type));
                    }
                }
            }
            return new SimulationResult(0, null);
        }
    }

}