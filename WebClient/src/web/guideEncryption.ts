// Die Verschlüsselung der Quelle
export enum guideEncryption {
    // Nur kostenlose Quellen
    FREE = 1,

    // Nur Bezahlsender
    PAY = 2,

    // Alle Sender
    ALL = FREE + PAY,
}
