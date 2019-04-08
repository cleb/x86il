mov ah, 4
mov bh, 255
add ah, bh
mov bl, 4
adc bl, [value]
value db 8
