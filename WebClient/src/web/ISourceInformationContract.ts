// Repräsentiert die Klasse SourceInformation
export interface ISourceInformationContract {
  // Der volle Name der Quelle
  name: string;

  // Die eindeutige Kennung der Quelle
  source?: string;

  // Gesetzt, wenn die Quelle verschlüsselt ist
  isEncrypted: boolean;
}
