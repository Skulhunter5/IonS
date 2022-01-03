using System;
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

        public Word[] GetWords() {
            var words = new List<Word>();
            Word word;
            while((word = NextWord()) != null) words.Add(word);
            return words.ToArray();
        }
    }

}