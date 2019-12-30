;jmp not implemented yet
cmp ax, ax
jz program
testdata db 0x42
program:
mov byte[testdata], 0x4
