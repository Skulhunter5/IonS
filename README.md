# IonS
IonS is a compiled stackbased programming language.\
Technically the "compiler" is just a transcriber that generates nasm x86_64 assembly though.\
\
IonS is currently written in C# but I want to make it a self-hosted language.

## Grammar
In IonS every operation is executed one after another, which results in something like ``` 1 2 + . ``` which outputs ```3```.
Step by step:
- ```1```: pushes 1 onto the stack\
Stack: [1]
- ```2```: pushes 2 onto the stack
Stack: [1, 2]
- ```+```: adds the two topmost items of the stack together
Stack: [3]
- ```.```: prints the top of the stack to stdout
Stack: [0]

### 
