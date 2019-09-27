mov bx, 4
mov ah, 127
mov dh, 128
cmp ah, dh
jl islower
mov bx, 8
islower:
mov ah, 4
mov dh, 4
jle islowerequal
mov bx, 15
islowerequal:
mov cx, 15