;jmp not implemented yet
cmp ax, ax
jz program
testdata dw 0xdead
program:
mov word[testdata], 0xbeef
