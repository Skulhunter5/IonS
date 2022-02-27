# Internal changes
- Redo stuff like ShLOperation.TypeCheck
- Move argument-, binding-, ... parsing into "parselist" function
- See if linear comparison of every object in a list is faster or slower than checking if a key exists inside a Dictionary (useful for Keyword.isKeyword())

# Implement for IonS
- look into capturing all words and strings through regex or maybe do list-parsing through regex
- Rework Error-Warning-System to maybe allow for single syntax error but multiple other errors and also allow warnings (see TypeChecker & ExitOperation)
- Rework words/tokens
- Implement nested bindings (maybe add something like a 'global nested counter' || recursive backtracking again but set the offset accordingly whenever the Scope.GetVariable() method is called)
- Implement function prototypes
- Implement ; as empty block
- Implement heap-allocation handler
- Implement structs
- Make switch statements viable (just assume a fallthrough at every point and then check if all endings have the same signature)
- (Implement automatic clearing of the stack when returning from functions)

# Implement in IonS

# restructure std.ions in to multiple files

# Notes
- Add HasSkipped to TypeCheckContract
- add c calls (e.g.: extern c proc ...)
- REWORK THE IMPLICIT CAST SYSTEM (e.g. everything can be cast to boolean without a warning (at least for an if, while, ...))

# TODO
- check that variable names aren't strings/chars
- check that variable names are actuallly valid
- normalize Incomplete/EOF errors
- make signature errors more readable
