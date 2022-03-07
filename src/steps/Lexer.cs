using System.Collections.Generic;

namespace IonS {

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

        public Word NextWord() {
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
                if(index == -1) ErrorSystem.AddError_i(new EOFInStringLiteralError(position));
            }

            index = FindWhiteSpace();
            if(index == -1) return new Word(position, _text.Substring(start, _text.Length - start));

            string text = _text.Substring(start, index - start);
            if(text.StartsWith('\'')) {
                if(!Utils.charRegex.IsMatch(text)) ErrorSystem.AddError_s(new InvalidCharError(new Word(position, text)));

                text = Utils.ConvertEscapeCharacters(text.Substring(1,text.Length-2), position);
                return new CharWord(position, text);
            }
            if(text.StartsWith('"')) {
                string type = Utils.GetStringType(text);
                if(type != "c" && type != "") ErrorSystem.AddError_s(new InvalidStringTypeError(type, Utils.GetNewPosition(text, position, text.Length - type.Length)));
                if(!raw) text = Utils.ConvertEscapeCharacters(text.Substring(1, text.Length - 2 - type.Length), position);
                return new StringWord(position, text, type);
            }

            return new Word(position, text);
        }

        public List<Word> run() {
            List<Word> words = new List<Word>();
            Word word = NextWord();
            while(word != null) {
                words.Add(word);
                word = NextWord();
            }
            return words;
        }

    }

}