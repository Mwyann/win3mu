BITS 16
org 100h


	mov		cx,10
loop1:
	mov		al,cl
	add		al,'0'-1
	mov		[counter],al
	mov		ah,09h
	mov		dx,hello
	int		21h
	loop	loop1
	ret
	

hello:
	db		"Hello World from Sharp86 - ("
counter:
	db		"0"
	db		")", 13, 10, "$"
