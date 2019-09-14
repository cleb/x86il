mov bx, 4
mov ah, 127
mov dh, 127
cmp ah, dh
jnz notzero
mov bx, 8
notzero:
mov cx, 15