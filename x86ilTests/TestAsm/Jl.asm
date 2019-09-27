mov bx, 4
mov ah, 127
mov dh, 128
cmp ah, dh
jl islower
mov bx, 8
islower:
mov cx, 15