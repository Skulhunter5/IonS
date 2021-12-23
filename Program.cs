using System;
using System.IO;
using System.Collections.Generic;

namespace IonS
{
    class Program
    {
        static void Main(string[] args) {
            if(args.Length == 0 || args[0] == "-i" || args[0] == "--interpret") Interpret();
            else if(args[0] == "--compile") {
                if(args.Length == 1) Compile("res/test.ions");
                else if(args.Length >= 2) {
                    if(args[1].StartsWith("\"")) {
                        Console.WriteLine("Unimplemented feature");
                        return;
                    }
                    Compile(args[1]);
                }
            } else if(args[0] == "-s" || args[0] == "--simulate") Simulate();
            else Console.WriteLine("Invalid argument: '" + args[0] + "'");
        }

        private static int CountLines(string text) {
            int count = 0;
            for(int i = 0; i < text.Length; i++) if(text[i] == '\n') count++;
            return count;
        }
        private static void Compile(string file) {
            var asmTscr = new AssemblyTranscriber(File.ReadAllText(file), file);
            AssemblyTranscriptionResult result = asmTscr.run();
            if(result.Error != null) {
                Console.WriteLine(result.Error);
                return;
            }
            if(result.Asm == null) {
                Console.WriteLine("No assembly has been generated.");
                return;
            }
            File.WriteAllText("\\\\wsl$\\Ubuntu-20.04\\shared\\test.ions.asm", result.Asm);
            Console.WriteLine("Generated assembly with " + CountLines(result.Asm) + " lines.");
        }

        private static void Interpret() {
            var simulator = new Simulator(100000);
            while(true) {
                Console.Write("> ");
                var line = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(line)) {
                    string msg = "[";
                    foreach(int i in simulator.stack) {
                        msg += i + ", ";
                    }
                    if(simulator.stack.Count > 0) msg = msg.Substring(0, msg.Length-2);
                    msg += "]";
                    Console.WriteLine(msg);
                }
                SimulationResult result = simulator.run(line, "<stdin>");
                if(result.Error != null) Console.WriteLine(result.Error);
                if(result.Exitcode != 0) Console.WriteLine("Exited with code " + result.Exitcode + ".");
            }
        }

        private static void Simulate() {
            SimulationResult result = new Simulator(100000).run(File.ReadAllText("res/test.ions"), "res/test.ions");
            if(result.Error != null) {
                Console.WriteLine(result.Error);
                return;
            }
            Console.WriteLine("Exited with code " + result.Exitcode + ".");
        }
    }

    class Position {
        public Position(string file, int line, int column) {
            File = file;
            Line = line;
            Column = column;
        }
        public override string ToString()
        {
            return File + ":" + Line + ":" + Column + ":";
        }
        public string File { get; }
        public int Line { get; }
        public int Column { get; }
    }

    abstract class Error {}
    abstract class ParserError : Error {
        public override string ToString()
        {
            return "[Parser]: ";
        }
    }
    sealed class InternalParserError : ParserError {
        public override string ToString()
        {
            return base.ToString() + "Internal Error";
        }
    }
    sealed class UnexpectedSymbolError : ParserError {
        public UnexpectedSymbolError(char symbol, Position position) {
            Symbol = symbol;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Unexpected symbol: '" + Symbol + "' at " + Position;
        }
        public char Symbol { get; }
        public Position Position { get; }
    }
    sealed class UnexpectedWordError : ParserError {
        public UnexpectedWordError(string text, Position position) {
            Text = text;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Unexpected word: '" + Text + "' at " + Position;
        }
        public string Text { get; }
        public Position Position { get; }
    }
    sealed class UnexpectedMarkerError : ParserError {
        public UnexpectedMarkerError(string text, Position position) {
            Text = text;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Unexpected marker: '" + Text + "' at " + Position;
        }
        public string Text { get; }
        public Position Position { get; }
    }
    sealed class IncompleteBlockError : ParserError {
        public IncompleteBlockError(string text, Position position) {
            Text = text;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Incomplete block: '" + Text + "' at " + Position;
        }
        public string Text { get; }
        public Position Position { get; }
    }
    abstract class SimulatorError : Error {
        public override string ToString()
        {
            return "[Simulator]: ";
        }
    }
    sealed class StackUnderflowError : SimulatorError {
        public StackUnderflowError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Stack underflow: '" + Text + "'";
        }
        public string Text { get; }
    }
    sealed class StackOverflowError : SimulatorError {
        public StackOverflowError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Stack overflow: '" + Text + "'";
        }
        public string Text { get; }
    }
    sealed class DivideByZeroError : SimulatorError {
        public DivideByZeroError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Division by 0: '" + Text + "'";
        }
        public string Text { get; }
    }
    sealed class UnimplementedOperationSimulatorError : SimulatorError {
        public UnimplementedOperationSimulatorError(OperationType type) {
            Type = type;
        }
        public override string ToString()
        {
            return base.ToString() + "Unimplemented operation: '" + Type + "'";
        }
        public OperationType Type { get; }
    }
    abstract class AssemblyTranscriberError : Error {
        public override string ToString()
        {
            return "[AssemblyTranscriber]: ";
        }
    }
    sealed class UnimplementedOperationAssemblyTranscriberError : SimulatorError {
        public UnimplementedOperationAssemblyTranscriberError(OperationType type) {
            Type = type;
        }
        public override string ToString()
        {
            return base.ToString() + "Unimplemented operation: '" + Type + "'";
        }
        public OperationType Type { get; }
    }

    class Word {
        public Word(Position position, string text) {
            Position = position;
            Text = text;
        }

        public Position Position { get; }
        public string Text { get; }
    }

    class Lexer {
        private readonly string _source;
        private readonly string _text;
        private int _position;
        private int _line, _column;

        public Lexer(string text, string source) {
            _text = text;
            _source = source;
        }

        private char c {
            get {
                if(_position >= _text.Length) return '\0';
                return _text[_position];
            }
        }

        private void Next() {
            _position++;
            _column++;
        }

        private void skipWhiteSpace() {
            while(char.IsWhiteSpace(c)) {
                if(c == '\n') {
                    _line++;
                    _column = 0;
                }
                Next();
            }
        }

        public Word NextWord() {
            if(_position < _text.Length) {
                skipWhiteSpace();
                Position position = new Position(_source, _line, _column);
                if(c == '\0') return null;
                int start = _position;
                while(!char.IsWhiteSpace(c) && c != '\0') Next();
                return new Word(position, _text.Substring(start, _position - start));
            }
            return null;
        }
    }

    enum OperationType {
        PushInteger,
        Add, Subtract, Multiply, Divide, Modulo,
        Dump,
        Drop, Drop2,
        Dup, Dup2,
        Over, Over2, Swap,
        Label, Jump, JumpIfZero, JumpIfNotZero,
        Exit,

        CodeBlock
    }

    abstract class Operation {
        public Operation(OperationType type) {
            Type = type;
        }
        public OperationType Type { get; }
    }
    sealed class PushIntegerOperation : Operation {
        public PushIntegerOperation(int value) : base(OperationType.PushInteger) {
            Value = value;
        }
        public int Value { get; }
    }
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
    sealed class DumpOperation : Operation {
        public DumpOperation() : base(OperationType.Dump) {}
    }
    sealed class ExitOperation : Operation {
        public ExitOperation() : base(OperationType.Exit) {}
    }
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
        public int Direction { get; }
    }
    sealed class JumpIfZeroOperation : Operation {
        public JumpIfZeroOperation(string label, int direction) : base(OperationType.JumpIfZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; }
    }
    sealed class JumpIfNotZeroOperation : Operation {
        public JumpIfNotZeroOperation(string label, int direction) : base(OperationType.JumpIfNotZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; }
    }

    abstract class Block {
        public Block(Position position, int id) {
            Position = position;
            Id = id;
        }
        public Position Position { get; }
        public int Id { get; }
    }
    sealed class IfBlock : Block {
        public IfBlock(Position position, int id) : base(position, id) {}
    }
    sealed class WhileBlock : Block {
        public WhileBlock(Position position, int id) : base(position, id) {}
        public Boolean HasDo { get; set; }
    }
    sealed class DoWhileBlock : Block {
        public DoWhileBlock(Position position, int id) : base(position, id) {}
        public Boolean HasWhile { get; set; }
    }

    class ParseResult {
        public ParseResult(List<Operation> operations, ParserError error) {
            Operations = operations;
            Error = error;
        }
        public List<Operation> Operations { get; }
        public ParserError Error { get; }
    }

    class Parser {
        private readonly Word[] _words;
        private int _position;

        private int nextControlStatementId = 0;

        public Parser(string text, string source)
        {
            var lexer = new Lexer(text, source);
            var words = new List<Word>();
            Word word;
            while((word = lexer.NextWord()) != null) words.Add(word);
            _words = words.ToArray();
        }

        private Word Peek(int offset) {
            var index = _position + offset;
            if(index >= _words.Length) return null;
            return _words[index];
        }

        private Word Current;

        private Word NextWord() {
            if(_position >= _words.Length) return null;
            Current = _words[_position++];
            return Current;
        }

        private int ControlStatementId() {
            return nextControlStatementId++;
        }

        public ParseResult Parse() {
            var operations = new List<Operation>();
            
            Stack<Block> openBlocks = new Stack<Block>();

            while(NextWord() != null) {
                if(Current.Text == "exit") {
                    operations.Add(new ExitOperation());
                } else if(Current.Text == "drop") {
                    operations.Add(new DropOperation());
                } else if(Current.Text == "2drop") {
                    operations.Add(new Drop2Operation());
                } else if(Current.Text == "dup") {
                    operations.Add(new DupOperation());
                } else if(Current.Text == "2dup") {
                    operations.Add(new Dup2Operation());
                } else if(Current.Text == "over") {
                    operations.Add(new OverOperation());
                } else if(Current.Text == "2over") {
                    operations.Add(new Over2Operation());
                } else if(Current.Text == "swap") {
                    operations.Add(new SwapOperation());
                } else if(Current.Text == "+") {
                    operations.Add(new AddOperation());
                } else if(Current.Text == "-") {
                    operations.Add(new SubtractOperation());
                } else if(Current.Text == "*") {
                    operations.Add(new MultiplyOperation());
                } else if(Current.Text == "/") {
                    operations.Add(new DivideOperation());
                } else if(Current.Text == "%") {
                    operations.Add(new ModuloOperation());
                } else if(Current.Text == ".") {
                    operations.Add(new DumpOperation());
                } else if(Current.Text == "if") { // TODO: add else and elif or anything similar
                    IfBlock ifBlock = new IfBlock(Current.Position, ControlStatementId());
                    openBlocks.Push(ifBlock);
                    operations.Add(new JumpIfZeroOperation("if_end_" + ifBlock.Id, 1));
                } else if(Current.Text == "while") {
                    if(openBlocks.Count > 0 && openBlocks.Peek().GetType() == typeof(DoWhileBlock) && !((DoWhileBlock) openBlocks.Peek()).HasWhile) {
                        DoWhileBlock doWhileBlock = (DoWhileBlock) openBlocks.Peek();
                        doWhileBlock.HasWhile = true;
                        operations.Add(new LabelOperation("dowhile_while_" + doWhileBlock.Id));
                    } else {
                        WhileBlock whileBlock = new WhileBlock(Current.Position, ControlStatementId());
                        openBlocks.Push(whileBlock);
                        operations.Add(new LabelOperation("while_while_" + whileBlock.Id));
                    }
                } else if(Current.Text == "do") {
                    Block block;
                    if(openBlocks.Count > 0 && (block = openBlocks.Peek()).GetType() == typeof(WhileBlock) && !((WhileBlock) block).HasDo) {
                        operations.Add(new JumpIfZeroOperation("while_end_" + block.Id, 1));
                        ((WhileBlock) block).HasDo = true;
                    } else {
                        DoWhileBlock doWhileBlock = new DoWhileBlock(Current.Position, ControlStatementId());
                        openBlocks.Push(doWhileBlock);
                        operations.Add(new LabelOperation("dowhile_do_" + doWhileBlock.Id));
                    }
                } else if(Current.Text == "end") {
                    if(openBlocks.Count == 0) return new ParseResult(null, new UnexpectedMarkerError(Current.Text, Current.Position));

                    Block block = openBlocks.Pop();
                    if(block.GetType() == typeof(IfBlock)) operations.Add(new LabelOperation("if_end_" + block.Id));
                    else if(block.GetType() == typeof(WhileBlock) && ((WhileBlock) block).HasDo) {
                        operations.Add(new JumpOperation("while_while_" + block.Id, -1));
                        operations.Add(new LabelOperation("while_end_" + block.Id));
                    } else if(block.GetType() == typeof(DoWhileBlock) && ((DoWhileBlock) block).HasWhile) {
                        operations.Add(new JumpIfNotZeroOperation("dowhile_do_" + block.Id, -1));
                        operations.Add(new LabelOperation("dowhile_end_" + block.Id));
                    } else return new ParseResult(null, new UnexpectedMarkerError(Current.Text, Current.Position));
                } else if(Current.Text == "continue") {
                    Block[] blocks = openBlocks.ToArray();
                    for(int i = blocks.Length-1; i >= 0; i--) {
                        if(blocks[i].GetType() == typeof(WhileBlock) && ((WhileBlock) blocks[i]).HasDo) {
                            operations.Add(new JumpOperation("while_while_" + blocks[i].Id, -1));
                            break;
                        } else if(blocks[i].GetType() == typeof(DoWhileBlock) && !((DoWhileBlock) blocks[i]).HasWhile) {
                            operations.Add(new JumpOperation("dowhile_while_" + blocks[i].Id, -1));
                            break;
                        }
                    }
                } else if(Current.Text == "break") {
                    Block[] blocks = openBlocks.ToArray();
                    for(int i = blocks.Length-1; i >= 0; i--) {
                        if(blocks[i].GetType() == typeof(WhileBlock) && ((WhileBlock) blocks[i]).HasDo) {
                            operations.Add(new JumpOperation("while_end_" + blocks[i].Id, 1));
                            break;
                        } else if(blocks[i].GetType() == typeof(DoWhileBlock) && !((DoWhileBlock) blocks[i]).HasWhile) {
                            operations.Add(new JumpOperation("dowhile_end_" + blocks[i].Id, 1));
                            break;
                        }
                    }
                } else {
                    if(int.TryParse(Current.Text, out int value)) operations.Add(new PushIntegerOperation(value));
                    else return new ParseResult(null, new UnexpectedWordError(Current.Text, Current.Position));
                }
            }
            if(openBlocks.Count > 0) {
                Block block = openBlocks.Pop();
                string text = "";
                if(block.GetType() == typeof(IfBlock)) text = "if";
                else if(block.GetType() == typeof(WhileBlock)) text = "while";
                else return new ParseResult(null, new InternalParserError());
                return new ParseResult(null, new IncompleteBlockError(text, block.Position));
            }

            return new ParseResult(operations, null);
        }
    }

    class SimulationResult {
        public SimulationResult(int exitcode, Error error) {
            Exitcode = exitcode;
            Error = error;
        }
        public int Exitcode { get; }
        public Error Error { get; }
    }

    class Simulator {
        public readonly Stack<int> stack;
        public readonly int maxStackSize;

        public Simulator(int maxStackSize) {
            stack = new Stack<int>();
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

                        return new SimulationResult(stack.Pop(), null);
                    }
                    case OperationType.PushInteger: {
                        PushIntegerOperation pushOperation = (PushIntegerOperation) operation;
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

                        int a = stack.Pop();
                        stack.Push(a);
                        stack.Push(a);
                        break;
                    }
                    case OperationType.Dup2: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("2dup"));
                        if(stack.Count >= maxStackSize - 1) return new SimulationResult(0, new StackOverflowError("2dup"));

                        int b = stack.Pop();
                        int a = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    }
                    case OperationType.Swap: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("swap"));

                        int a = stack.Pop();
                        int b = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    }
                    case OperationType.Over: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("over"));
                        if(stack.Count >= maxStackSize) return new SimulationResult(0,  new StackOverflowError("over"));

                        int b = stack.Pop();
                        int a = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(a);
                        break;
                    }
                    case OperationType.Over2: {
                        if(stack.Count <= 2) return new SimulationResult(0, new StackUnderflowError("2over"));
                        if(stack.Count >= maxStackSize - 1) return new SimulationResult(0, new StackOverflowError("2over"));

                        int d = stack.Pop();
                        int c = stack.Pop();
                        int b = stack.Pop();
                        int a = stack.Pop();
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

                        int b = stack.Pop();
                        int a = stack.Pop();
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

                        int b = stack.Pop();
                        if(b == 0) return new SimulationResult(0, new DivideByZeroError("/"));

                        int a = stack.Pop();
                        stack.Push(a / b);
                        break;
                    }
                    case OperationType.Modulo: {
                        if(stack.Count <= 1) return new SimulationResult(0, new StackUnderflowError("%"));

                        int b = stack.Pop();
                        if(b == 0) return new SimulationResult(0, new DivideByZeroError("%"));

                        int a = stack.Pop();
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

    class AssemblyTranscriptionResult {
        public AssemblyTranscriptionResult(string asm, Error error) {
            Asm = asm;
            Error = error;
        }
        public string Asm { get; }
        public Error Error { get; }
    }

    class AssemblyTranscriber {
        private readonly string _text, _source;
        public AssemblyTranscriber(string text, string source) {
            _text = text;
            _source = source;
        }

        public AssemblyTranscriptionResult run() {
            string asm = File.ReadAllText("res/asm snippets/start.asm");
            var parser = new Parser(_text, _source);
            var result = parser.Parse();
            if(result.Error != null) return new AssemblyTranscriptionResult(null, result.Error);
            var operations = result.Operations;
            for(int i = 0; i < operations.Count; i++) {
                Operation operation = operations[i];
                switch(operation.Type) {
                    case OperationType.Exit: {
                        if(i != operations.Count - 1) asm += "    jmp exit\n"; // TODO: move into a dedicated optimizer
                        break;
                    }
                    case OperationType.PushInteger: {
                        asm += $"    push {((PushIntegerOperation) operation).Value}\n";
                        break;
                    }
                    case OperationType.Drop: {
                        asm += "    pop rax\n";
                        break;
                    }
                    case OperationType.Drop2: {
                        asm += "    pop rax\n";
                        asm += "    pop rax\n";
                        break;
                    }
                    case OperationType.Dup: {
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Dup2: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        break;
                    }
                    case OperationType.Swap: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Over: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Over2: {
                        asm += "    pop rdx\n";
                        asm += "    pop rcx\n";
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rcx\n";
                        asm += "    push rdx\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        break;
                    }
                    case OperationType.Add: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    add rax, rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Subtract: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    sub rax, rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Multiply: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    mul rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Divide: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    xor rdx, rdx\n"; // TODO: check why this is necessary
                        asm += "    div rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Modulo: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    xor rdx, rdx\n";
                        asm += "    div rbx\n";
                        asm += "    push rdx\n";
                        break;
                    }
                    case OperationType.Dump: {
                        asm += "    pop rdi\n";
                        asm += "    call print\n";
                        break;
                    }
                    case OperationType.Label: {
                        asm += ((LabelOperation) operation).Label + ":\n";
                        break;
                    }
                    case OperationType.Jump: {
                        asm += "    jmp " + ((JumpOperation) operation).Label + "\n";
                        break;
                    }
                    case OperationType.JumpIfZero: {
                        asm += "    pop rax\n";
                        asm += "    cmp rax, 0\n";
                        asm += "    je " + ((JumpIfZeroOperation) operation).Label + "\n";
                        break;
                    }
                    case OperationType.JumpIfNotZero: {
                        asm += "    pop rax\n";
                        asm += "    cmp rax, 0\n";
                        asm += "    jne " + ((JumpIfNotZeroOperation) operation).Label + "\n";
                        break;
                    }
                    default: {
                        return new AssemblyTranscriptionResult(null, new UnimplementedOperationAssemblyTranscriberError(operation.Type));
                    }
                }
            }
            asm += "exit:\n";
            asm += "    mov rax, 60\n";
            asm += "    pop rdi\n";
            asm += "    syscall\n";
            return new AssemblyTranscriptionResult(asm, null);
        }
    }
}
