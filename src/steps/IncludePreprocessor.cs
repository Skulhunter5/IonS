using System.IO;
using System.Collections.Generic;

using System;

namespace IonS {

    class IncludePreprocessorResult : Result {
        public IncludePreprocessorResult(Word[] words, Error error) : base(error) {
            Words = words;
        }
        public Word[] Words { get; }
    }

    class IncludePreprocessor {

        private readonly string _source;
        private readonly Word[] _words;

        public IncludePreprocessor(string source, Word[] words) {
            _words = words;
            _source = source;
        }

        private string AlterPath(string original, string alterBy) {
            string temp = "";

            temp = Path.GetDirectoryName(original);
            string[] steps = alterBy.Split('/');
            if(steps[0] != ".") return alterBy;
            for(int i = 1; i < steps.Length; i++) {
                string step = steps[i];
                if(step == "..") temp = new DirectoryInfo(temp).Parent.ToString();
                else temp += "\\" + step;
            }

            return temp;
        }

        public IncludePreprocessorResult run() {
            List<Word> words = new List<Word>();
            List<int> includes = new List<int>();
            for(int j = 0; j < _words.Length; j++) if(_words[j].Text == "include") includes.Add(j);
            if(includes.Count == 0) return new IncludePreprocessorResult(_words, null);
            if(includes[includes.Count-1] >= _words.Length - 1) return new IncludePreprocessorResult(null, new IncompleteIncludeError(_words[_words.Length-1].Position));

            int i = 0;
            while(i < includes.Count) {
                for(int j = (i > 0 ? includes[i-1]+2 : 0); j < includes[i]; j++) words.Add(_words[j]);
                Word nameWord = _words[includes[i]+1];
                if(!nameWord.Text.StartsWith('"') || !nameWord.Text.EndsWith('"')) return new IncludePreprocessorResult(null, new FilePathNotAStringLiteralError(nameWord));
                string filename = AlterPath(_source, nameWord.Text.Substring(1, nameWord.Text.Length-2));
                
                if(!File.Exists(filename)) return new IncludePreprocessorResult(null, new FileNotFoundError(filename, Directory.GetCurrentDirectory(), nameWord.Position));
                string path = Path.GetFullPath(filename);
                string text = File.ReadAllText(path).Replace("\r\n", "\n");
                var lexingResult = new Lexer(text, path).run();
                if(lexingResult.Error != null) return new IncludePreprocessorResult(null, lexingResult.Error);
                var incPreprocResult = new IncludePreprocessor(path, lexingResult.Words).run();
                if(incPreprocResult.Error != null) return new IncludePreprocessorResult(null, incPreprocResult.Error);
                foreach(Word word in incPreprocResult.Words) {
                    Word includedFrom = _words[includes[i]+1];
                    if(word.IncludedFrom == null) word.IncludedFrom = new Word(includedFrom.Position, includedFrom.Text.Substring(1, includedFrom.Text.Length-2));
                    words.Add(word);
                }
                i++;
            }
            for(int j = includes[i-1]+2; j < _words.Length; j++) words.Add(_words[j]);

            return new IncludePreprocessorResult(words.ToArray(), null);
        }

    }

}