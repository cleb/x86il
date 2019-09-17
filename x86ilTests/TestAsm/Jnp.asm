mov bx, 4
mov ah, 127
mov dh, 122
sub ah, dh
jnp hasparity
mov bx, 8
hasparity:
mov cx, 15