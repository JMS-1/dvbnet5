﻿// Muss vom Client mit dem relativen Pfad zu den HTTP REST Web Diensten belegt werden.
export let webCallRoot: string

export function setWebCallRoot(root: string): void {
    webCallRoot = root
}

// Ein Zähler für HTTP Aufrufe - veraltete Antworten werden in der aktuellen Implementierung grundsätzlich ignoriert.
let webCallId = 0

// Meldet einen neuen Zähler für eine folgenden HTTP Aufruf.
export function nextWebCallId(): number {
    return ++webCallId
}

// Meldet den aktuellen für eine folgenden HTTP Aufruf.
export function currentWebCallId(): number {
    return webCallId
}

// Stellt sicher, dass alle noch eintreffenden Antworten auf HHTP Aufrufe ignoriert werden.
export function switchView(): void {
    nextWebCallId()
}
