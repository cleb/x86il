mov bx, 4
mov ax, 8
push ax
mov ax, 15
push ax
mov ax, destination
push ax
retn
mov bx, 16
destination:
pop ax