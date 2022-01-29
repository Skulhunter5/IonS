namespace IonS {

    class Preprocessor {

        private readonly string _source;
        readonly private Word[] _words;

        public Preprocessor(string source, Word[] words) {
            _source = source;
            _words = words;
        }

        public PreprocessorResult run() {
            var commentResult = new CommentPreprocessor(_words).run();
            if(commentResult.Error != null) return new PreprocessorResult(null, commentResult.Error);

            var includeResult = new IncludePreprocessor(_source, commentResult.Words).run();
            if(includeResult.Error != null) return new PreprocessorResult(null, includeResult.Error);

            var macroResult = new MacroPreprocessor(includeResult.Words).run();
            if(macroResult.Error != null) return new PreprocessorResult(null, macroResult.Error);
            
            return new PreprocessorResult(macroResult.Words, null);
        }

    }

}