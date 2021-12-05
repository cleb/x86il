org 100h
start:
push cs
pop ds
mov ax, 4815
mov bx, 2342
add ax, bx
jo overflow
mov ah, 9
mov dx, nooverflowmsg
int 21h
jmp end
overflow:
mov ah, 9
mov dx, overflowmsg
int 21h
end:
mov ah, 4ch
mov al, 0
int 21h


nooverflowmsg db "no overflow",13,10,"$"
overflowmsg db "overflow",13,10,"$"
