using System.Collections.Generic;
using System.Text;

using System;

namespace IonS {

    class ParseResult : Result {
        public ParseResult(List<Variable> variables, List<string> strings, List<Operation> operations, Error error) : base(error) {
            Variables = variables;
            Strings = strings;
            Operations = operations;
        }
        public List<Variable> Variables { get; }
        public List<string> Strings { get; }
        public List<Operation> Operations { get; }
    }

    class Parser {
        private readonly string _text, _source;
        private Word[] _words;
        private int _position;

        private List<Variable> _vars;
        private List<string> _strings;

        private int nextControlStatementId = 0;

        public Parser(string text, string source)
        {
            _text = text;
            _source = source;
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

        private Variable GetVariable(string identifier) {
            foreach(Variable var in _vars) if(var.Identifier.Text == identifier) return var;
            return null;
        }

        public ParseResult Parse() {
            var lexingResult = new Lexer(_text, _source).run();
            if(lexingResult.Error != null) return new ParseResult(null, null, null, lexingResult.Error);
            _words = lexingResult.Words;

            var incPreprocResult = new IncludePreprocessor(_source, _words).run();
            if(incPreprocResult.Error != null) return new ParseResult(null, null, null, incPreprocResult.Error);
            _words = incPreprocResult.Words;

            var result = new MacroPreprocessor(_words).run();
            if(result.Error != null) return new ParseResult(null, null, null, result.Error);
            _words = result.Words;

            _vars = new List<Variable>();
            _strings = new List<string>();

            var operations = new List<Operation>();
            
            Stack<Block> openBlocks = new Stack<Block>();

            while(NextWord() != null) {
                if(Current.Text == "exit") {
                    operations.Add(new ExitOperation());
                } else if(Current.Text == "putc") {
                    operations.Add(new Put_char_Operation());
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
                } else if(Current.Text == "2swap") {
                    operations.Add(new Swap2Operation());
                } else if(Current.Text == "rot") {
                    operations.Add(new RotateOperation());
                } else if(Current.Text == "2rot") {
                    operations.Add(new Rotate2Operation());
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
                } else if(Current.Text == "/%") {
                    operations.Add(new DivModOperation());
                } else if(Current.Text == "<<") {
                    operations.Add(new ShLOperation());
                } else if(Current.Text == ">>") {
                    operations.Add(new ShROperation());
                } else if(Current.Text == "&") {
                    operations.Add(new BitAndOperation());
                } else if(Current.Text == "|") {
                    operations.Add(new BitOrOperation());
                } else if(Current.Text == "min") {
                    operations.Add(new MinOperation());
                } else if(Current.Text == "max") {
                    operations.Add(new MaxOperation());
                } else if(Current.Text == "==") {
                    operations.Add(new ComparisonOperation(ComparisonType.EQ));
                } else if(Current.Text == "!=") {
                    operations.Add(new ComparisonOperation(ComparisonType.NEQ));
                } else if(Current.Text == "<") {
                    operations.Add(new ComparisonOperation(ComparisonType.B));
                } else if(Current.Text == ">") {
                    operations.Add(new ComparisonOperation(ComparisonType.A));
                } else if(Current.Text == "<=") {
                    operations.Add(new ComparisonOperation(ComparisonType.BEQ));
                } else if(Current.Text == ">=") {
                    operations.Add(new ComparisonOperation(ComparisonType.AEQ));
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
                    if(openBlocks.Count == 0) return new ParseResult(null, null, null, new UnexpectedMarkerError(Current));

                    Block block = openBlocks.Pop();
                    if(block.GetType() == typeof(IfBlock)) operations.Add(new LabelOperation("if_end_" + block.Id));
                    else if(block.GetType() == typeof(WhileBlock) && ((WhileBlock) block).HasDo) {
                        operations.Add(new JumpOperation("while_while_" + block.Id, -1));
                        operations.Add(new LabelOperation("while_end_" + block.Id));
                    } else if(block.GetType() == typeof(DoWhileBlock) && ((DoWhileBlock) block).HasWhile) {
                        operations.Add(new JumpIfNotZeroOperation("dowhile_do_" + block.Id, -1));
                        operations.Add(new LabelOperation("dowhile_end_" + block.Id));
                    } else return new ParseResult(null, null, null, new UnexpectedMarkerError(Current));
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
                } else if(Current.Text == "var") {
                    Word varWord = Current;
                    NextWord();
                    if(Current == null) return new ParseResult(null, null, null, new IncompleteVariableDeclarationError(varWord, null));

                    Word identifier = Current;
                    // TODO: Check that the identifier is valid for nasm aswell
                    if(Keyword.isReserved(identifier.Text) || long.TryParse(identifier.Text, out long _)) return new ParseResult(null, null, null, new InvalidVariableIdentifierError(identifier));
                    Variable var = GetVariable(identifier.Text);
                    if(var != null) return new ParseResult(null, null, null, new VariableRedeclarationError(var.Identifier, identifier));

                    NextWord();
                    if(Current.Text == null) return new ParseResult(null, null, null, new IncompleteVariableDeclarationError(varWord, identifier));

                    if(byte.TryParse(Current.Text, out byte bytesize)) _vars.Add(new Variable(identifier, bytesize));
                    else return new ParseResult(null, null, null, new InvalidVariableBytesizeError(Current));
                } else if(Current.Text.StartsWith("!")) {
                    string amountStr = Current.Text.Substring(1);
                    bool isByte = byte.TryParse(amountStr, out byte amount);
                    if(!isByte) return new ParseResult(null, null, null, new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));

                    if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new ParseResult(null, null, null, new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));

                    operations.Add(new MemWriteOperation(amount));
                } else if(Current.Text.StartsWith("@")) {
                    string amountStr = Current.Text.Substring(1);
                    bool isByte = byte.TryParse(amountStr, out byte amount);
                    if(!isByte) return new ParseResult(null, null, null, new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));

                    if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new ParseResult(null, null, null, new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));

                    operations.Add(new MemReadOperation(amount));
                } else if(Current.Text.StartsWith('"')) {
                    if(Current.Text.EndsWith("\"")) {
                        operations.Add(new StringOperation(_strings.Count));
                        _strings.Add(Current.Text.Substring(1, Current.Text.Length - 2));
                    } else if(Current.Text.EndsWith("\"c")) {
                        operations.Add(new CStyleStringOperation(_strings.Count));
                        _strings.Add(Current.Text.Substring(1, Current.Text.Length - 3) + "\0");
                    }
                } else if(Current.Text.StartsWith("'")) {
                    string text = Current.Text.Substring(1, Current.Text.Length-2);
                    char c = text[0];
                    if(text.Length == 2) {
                        // TODO: factor out into function
                        c = text[1];
                        if(c == 'n') c = '\n';
                        else if(c == 't') c = '\t';
                        else if(c == 'r') c = '\r';
                        else if(c == '\\') c = '\\';
                        else if(c == '"') c = '"';
                        else if(c == '0') c = '\0';
                        else if(c == '\n') return new ParseResult(null, null, null, new InvalidEscapeCharacterError("\\n", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                        else if(c == '\t') return new ParseResult(null, null, null, new InvalidEscapeCharacterError("\\t", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                        else if(c == '\r') return new ParseResult(null, null, null, new InvalidEscapeCharacterError("\\r", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                        else return new ParseResult(null, null, null, new InvalidEscapeCharacterError(""+c, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                    }
                    operations.Add(new Push_uint64_Operation(Encoding.ASCII.GetBytes(""+c)[0]));
                } else if(Current.Text == "here") {
                    operations.Add(new StringOperation(_strings.Count));
                    _strings.Add(""+Current.Position);
                } else if(Current.Text == "chere") {
                    operations.Add(new CStyleStringOperation(_strings.Count));
                    _strings.Add(Current.Position + "\0");
                } else if(Current.Text.StartsWith("syscall")) {
                    string argcStr = Current.Text.Substring(7, Current.Text.Length - 7);
                    if(!int.TryParse(argcStr, out int argc)) return new ParseResult(null, null, null, new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7)));
                    if(argc >= 0 && argc <= 6) operations.Add(new SyscallOperation(argc));
                    else return new ParseResult(null, null, null, new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7)));
                } else {
                    if(ulong.TryParse(Current.Text, out ulong value)) operations.Add(new Push_uint64_Operation(value));
                    else {
                        Variable var = GetVariable(Current.Text);
                        if(var != null) operations.Add(new VariableAccessOperation(var.Identifier.Text));
                        else return new ParseResult(null, null, null, new UnexpectedWordError(Current));
                    }
                }
            }
            if(openBlocks.Count > 0) return new ParseResult(null, null, null, new IncompleteBlockError(openBlocks.Pop()));

            return new ParseResult(_vars, _strings, operations, null);
        }
    }

}