mov bx, 4
mov ah, 127
mov dh, 128
sub ah, dh
js hascarry
mov bx, 8
hascarry:
mov cx, 15