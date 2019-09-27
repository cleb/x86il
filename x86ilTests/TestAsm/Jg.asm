mov bx, 4
mov ah, 128
mov dh, 127
cmp ah, dh
jg ishigher
mov bx, 8
ishigher:
mov cx, 15