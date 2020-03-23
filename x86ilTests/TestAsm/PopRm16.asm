;jmp not implemented yet
cmp ax, ax
jz program
testdata dw 0xdead
program:
mov ax, 0xbeef
push ax
pop word[testdata]
