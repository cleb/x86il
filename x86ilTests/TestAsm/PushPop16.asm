mov ax, 4
mov bx, 8
mov cx, 15
mov dx, 16
mov si, 23
mov di, 42
mov bp, 3
mov sp, afterstackspace
afterstackspace:
push ax
push bx
push cx
push dx
push si
push di
push bp
pop ax
pop bx
pop cx
pop dx
pop bp
pop si
pop di