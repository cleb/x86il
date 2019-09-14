mov bx, 4
mov ah, 127
mov dh, 127
cmp ah, dh
jz iszero
mov bx, 8
iszero:
mov cx, 15