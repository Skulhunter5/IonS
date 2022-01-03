using System;
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

        public SingleWordResult NextWord() {
            if(_position < _text.Length) {
                skipWhiteSpace();
                if(c == '\0') return null;
                Position position = new Position(_source, _line, _column);
                int start = _position;
                int end = 0;
                Position endPos = null;
                if(c == '"') {
                    string text = "";
                    Next();
                    while(c != '\0' && c != '"') {
                        if(c == '\\') {
                            Next();
                            if(c == '\0') return new SingleWordResult(null, new EOFInStringLiteralError(position));
                            else if(c == 'n') text += '\n';
                            else if(c == 't') text += '\t';
                            else if(c == 'r') text += '\r';
                            else if(c == '\\') text += '\\';
                            else if(c == '"') text += '"';
                            else if(c == '0') text += '\0';
                            else if(c == '\n') return new SingleWordResult(null, new InvalidEscapeCharacterError("\\n", new Position(_source, _line, _column)));
                            else if(c == '\t') return new SingleWordResult(null, new InvalidEscapeCharacterError("\\t", new Position(_source, _line, _column)));
                            else if(c == '\r') return new SingleWordResult(null, new InvalidEscapeCharacterError("\\r", new Position(_source, _line, _column)));
                            else return new SingleWordResult(null, new InvalidEscapeCharacterError(""+c, new Position(_source, _line, _column)));
                        } else text += c;
                        Next();
                    }
                    Next();
                    if(start + (_position - start) > _text.Length) return new SingleWordResult(null, new EOFInStringLiteralError(position));

                    end = _position;

                    while(!char.IsWhiteSpace(c) && c != '\0') Next();

                    if(end == _position) return new SingleWordResult(new Word(position, '"' + text + '"'), null);

                    string type = _text.Substring(end, _position - end);
                    if(type == "c") text += '\0';
                    else return new SingleWordResult(null, new InvalidStringTypeError(type, endPos));
                    return new SingleWordResult(new Word(position, '"' + text + '"'), null);
                } else {
                    while(!char.IsWhiteSpace(c) && c != '\0') Next();
                    int len = _position - start;
                    return new SingleWordResult(new Word(position, _text.Substring(start, len)), null);
                }
            }
            return null;
        }

        public LexingResult GetWords() {
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