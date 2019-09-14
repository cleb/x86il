mov bx, 4
mov ah, 127
mov dh, 127
cmp ah, dh
jbe iszero
mov bx, 8
iszero:
mov dh, 128
sub ah, dh
jbe isbelow
mov bx, 15
isbelow:
mov cx, 15