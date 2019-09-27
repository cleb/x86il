mov bx, 4
mov ah, 127
mov dh, 128
cmp ah, dh
jnl isnotlower
mov bx, 8
isnotlower:
mov cx, 15