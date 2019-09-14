mov bx, 4
mov ah, 127
mov dh, 127
add ah, dh
jno hasoverflow
mov bx, 8
hasoverflow:
mov cx, 15