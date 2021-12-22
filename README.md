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
- ```2```: pushes 2 onto the stack\
Stack: [1, 2]
- ```+```: adds the two topmost items and pushes the result back onto the stack\
Stack: [3]
- ```.```: prints the top of the stack to stdout\
Stack: []
## Operations
### just a simple integer such as ```0```, ```1``` or ```12345``` so generally ```n```.
Pushes that integer onto the stack.
```
Push(n)
```
### ```+```
Adds the topmost 2 items and pushes the result\
```
b = Pop()
a = Pop()
Push(a + b)
```
### ```-```
Subtracts the topmost 2 items and pushes the result
```
b = Pop()
a = Pop()
Push(a - b)
```
### ```*```
Multiplies the topmost 2 items and pushes the result back onto the stack\
```
b = Pop()
a = Pop()
Push(a * b)
```
### ```/```
Divides the topmost 2 items and pushes the result back onto the stack
```
b = Pop()
a = Pop()
Push(a / b)
```
