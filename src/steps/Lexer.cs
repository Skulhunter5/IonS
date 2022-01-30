using System.Collections.Generic;

namespace IonS {

    class LexingResult : Result {
        public LexingResult(Word[] words, Error error) : base(error) {
            Words = words;
        }
        public Word[] Words { get; }
    }

    class SingleWordResult : Result {
        public SingleWordResult(Word word, Error error) : base(error) {
            Word = word;
        }
        public Word Word { get; }
    }

    class Lexer {
        private readonly string _source;
        private readonly string _text;
        private int _position;
        private int _line = 1, _column = 1;

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

        private char GetChar(int offset) {
            if(_position+offset >= _text.Length) return '\0';
            return _text[_position+offset];
        }

        private void Next() {
            _position++;
            _column++;
        }

        private void SkipWhiteSpace() {
            while(char.IsWhiteSpace(c)) {
                if(c == '\n') {
                    _line++;
                    _column = 0;
                }
                Next();
            }
        }

        private int FindWhiteSpace() {
            while(!char.IsWhiteSpace(c) && c != '\0') Next();
            return c != '\0' ? _position : -1;
        }

        private int Find(char toFind) {
            while(c != toFind && c != '\0') {
                if(c == '\n') {
                    _line++;
                    _column = 0;
                }
                Next();
            }
            return c != '\0' ? _position : -1;
        }

        private int FindDoubleQuote(bool raw) {
            char last = '\0';
            while((c != '"' || (last == '\\' || raw)) && c != '\0') {
                if(c == '\n') {
                    _line++;
                    _column = 0;
                }
                last = c;
                Next();
            }
            return c != '\0' ? _position : -1;
        }

        public SingleWordResult NextWord() {
            SkipWhiteSpace();
            if(_position >= _text.Length) return null;

            Position position = new Position(_source, _line, _column);
            int start = _position;
            
            int index = 0;
            bool raw = false;
            if(c == 'r' && GetChar(1) == '"') {
                raw = true;
                Next();
            }
            if(c == '"') {
                Next();
                index = FindDoubleQuote(raw);
                if(index == -1) return new SingleWordResult(null, new EOFInStringLiteralError(position));
            }

            index = FindWhiteSpace();
            if(index == -1) return new SingleWordResult(new Word(position, _text.Substring(start, _text.Length - start)), null);

            string text = _text.Substring(start, index - start);
            if(text.StartsWith('\'')) {
                if(text.Length == 3 && text[2] != '\'') return new SingleWordResult(null, new InvalidCharError(new Word(position, _text.Substring(start, text.Length))));
                if(text.Length == 4 && (text[1] != '\\' || text[3] != '\'')) return new SingleWordResult(null, new InvalidCharError(new Word(position, _text.Substring(start, text.Length))));
                if(text.Length != 3 && text.Length != 4) return new SingleWordResult(null, new InvalidCharError(new Word(position, _text.Substring(start, text.Length))));

                var result = Utils.ConvertEscapeCharacters(text.Substring(1,text.Length-2), position);
                if(result.Error != null) return new SingleWordResult(null, result.Error);
                return new SingleWordResult(new CharWord(position, result.Text), null);
            }
            if(text.StartsWith('"')) {
                string type = Utils.GetStringType(text);
                if(type != "c" && type != "") return new SingleWordResult(null, new InvalidStringTypeError(type, Utils.GetNewPosition(text, position, text.Length - type.Length)));
                if(!raw) {
                    var result = Utils.ConvertEscapeCharacters(text.Substring(1, text.Length - 2 - type.Length), position);
                    if(result.Error != null) return new SingleWordResult(null, result.Error);
                    text = result.Text;
                }
                return new SingleWordResult(new StringWord(position, text, type), null);
            }

            return new SingleWordResult(new Word(position, text), null);
        }

        public LexingResult run() {
            var words = new List<Word>();
            SingleWordResult result = NextWord();
            while(result != null) {
                if(result.Error != null) return new LexingResult(null, result.Error);
                words.Add(result.Word);
                result = NextWord();
            }
            return new LexingResult(words.ToArray(), null);
        }

    }

}