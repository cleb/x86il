;jmp not implemented yet
cmp ax, ax
jz program
testdata dd 0xdeadbeef
program:
mov bx, testdata
les di, [bx]
