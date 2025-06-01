import { IPage, Page } from "./page";

import { Command, ICommand } from "../../lib/command/command";
import { BooleanProperty, IFlag } from "../../lib/edit/boolean/flag";
import * as list from "../../lib/edit/list";
import { INumber, NumberProperty } from "../../lib/edit/number/number";
import * as profileContract from "../../web/IUserProfileContract";
import { Application } from "../app";

// Schnittstelle zur Pflege der Benutzereinstellung.
export interface ISettingsPage extends IPage {
  // Die Anzahl von Tagen im Aufzeichnungsplan.
  readonly planDays: INumber;

  // Die maximale Anzahl von Quellen in der Liste zuletzt verwendeter Quellen.
  readonly maxFavorites: INumber;

  // Die Anzahl der Einträge auf einer Seite der Programmzeitschrift.
  readonly guideRows: INumber;

  // Die Vorlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
  readonly preGuide: INumber;

  // Die Nachlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
  readonly postGuide: INumber;

  // Gesetzt, wenn bevorzugt das Dolby-Digital Tonsignal mit aufgezeichnet werden soll.
  readonly dolby: IFlag;

  // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen.
  readonly allAudio: IFlag;

  // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll.
  readonly ttx: IFlag;

  // Gesetzt, wenn bevorzugt die DVB Untertitel mit aufgezeichnet werden soll.
  readonly subs: IFlag;

  // Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift in diese zurück gekehrt werden soll.
  readonly backToGuide: IFlag;

  // Die bevorzugte Einschränkung auf die Art der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
  readonly sourceType: list.IValueFromList<string>;

  // Die bevorzugte Einschränkung auf die Verschlüsselung der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
  readonly encryption: list.IValueFromList<string>;

  // Befehl zur Aktualisierung der Einstellungen.
  readonly update: ICommand;
}

// Präsentationsmodell zur Pflege der Einstellungen des Anwenders.
export class SettingsPage extends Page implements ISettingsPage {
  // Alle Einschränkungen auf die Art der Quellen.
  private static readonly _types = [
    list.uiValue("RT", "Alle Quellen"),
    list.uiValue("R", "Nur Radio"),
    list.uiValue("T", "Nur Fernsehen"),
  ];

  // Alle Einschränkungen auf die Verschlüsselung der Quellen.
  private static readonly _encryptions = [
    list.uiValue("FP", "Alle Quellen"),
    list.uiValue("P", "Nur verschlüsselte Quellen"),
    list.uiValue("F", "Nur unverschlüsselte Quellen"),
  ];

  // Befehl zur Aktualisierung der Einstellungen.
  readonly update = new Command(
    () => this.save(),
    "Aktualisieren",
    () => this.isValid
  );

  // Die Anzahl von Tagen im Aufzeichnungsplan.
  readonly planDays = new NumberProperty(
    {} as Pick<profileContract.IUserProfileContract, "planDays">,
    "planDays",
    "Anzahl der Vorschautage im Aufzeichnungsplan",
    () => this.update.refreshUi()
  )
    .addRequiredValidator()
    .addMinValidator(1)
    .addMaxValidator(50);

  // Die maximale Anzahl von Quellen in der Liste zuletzt verwendeter Quellen.
  readonly maxFavorites = new NumberProperty(
    {} as Pick<profileContract.IUserProfileContract, "recentSourceLimit">,
    "recentSourceLimit",
    "Maximale Größe der Liste zuletzt verwendeter Sendern",
    () => this.update.refreshUi()
  )
    .addRequiredValidator()
    .addMinValidator(1)
    .addMaxValidator(50);

  // Die Anzahl der Einträge auf einer Seite der Programmzeitschrift.
  readonly guideRows = new NumberProperty(
    {} as Pick<profileContract.IUserProfileContract, "guideRows">,
    "guideRows",
    "Anzahl der Einträge pro Seite in der Programmzeitschrift",
    () => this.update.refreshUi()
  )
    .addRequiredValidator()
    .addMinValidator(10)
    .addMaxValidator(100);

  // Die Vorlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
  readonly preGuide = new NumberProperty(
    {} as Pick<profileContract.IUserProfileContract, "guideAheadStart">,
    "guideAheadStart",
    "Vorlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)",
    () => this.update.refreshUi()
  )
    .addRequiredValidator()
    .addMinValidator(0)
    .addMaxValidator(240);

  // Die Nachlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
  readonly postGuide = new NumberProperty(
    {} as Pick<profileContract.IUserProfileContract, "guideBeyondEnd">,
    "guideBeyondEnd",
    "Nachlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)",
    () => this.update.refreshUi()
  )
    .addRequiredValidator()
    .addMinValidator(0)
    .addMaxValidator(240);

  // Gesetzt, wenn bevorzugt das Dolby-Digital Tonsignal mit aufgezeichnet werden soll.
  readonly dolby = new BooleanProperty(
    {} as Pick<profileContract.IUserProfileContract, "dolby">,
    "dolby",
    "Dolby Digital (AC3)"
  );

  // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen.
  readonly allAudio = new BooleanProperty(
    {} as Pick<profileContract.IUserProfileContract, "languages">,
    "languages",
    "Alle Sprachen"
  );

  // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll.
  readonly ttx = new BooleanProperty(
    {} as Pick<profileContract.IUserProfileContract, "videotext">,
    "videotext",
    "Videotext"
  );

  // Gesetzt, wenn bevorzugt die DVB Untertitel mit aufgezeichnet werden soll.
  readonly subs = new BooleanProperty(
    {} as Pick<profileContract.IUserProfileContract, "subtitles">,
    "subtitles",
    "DVB Untertitel"
  );

  // Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift in diese zurück gekehrt werden soll.
  readonly backToGuide = new BooleanProperty(
    {} as Pick<profileContract.IUserProfileContract, "backToGuide">,
    "backToGuide",
    "Nach Anlegen einer neuen Aufzeichnung zurück zur Programmzeitschrift"
  );

  // Die bevorzugte Einschränkung auf die Art der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
  readonly sourceType = new list.SingleListProperty(
    {} as Pick<profileContract.IUserProfileContract, "typeFilter">,
    "typeFilter",
    undefined,
    undefined,
    SettingsPage._types
  ).addRequiredValidator();

  // Die bevorzugte Einschränkung auf die Verschlüsselung der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
  readonly encryption = new list.SingleListProperty(
    {} as Pick<profileContract.IUserProfileContract, "encryptionFilter">,
    "encryptionFilter",
    undefined,
    undefined,
    SettingsPage._encryptions
  ).addRequiredValidator();

  // Erstellt ein neues Präsentationsmodell.
  constructor(application: Application) {
    super("settings", application);
  }

  // Initialisiert das Präsentationsmodell.
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  reset(sections: string[]): void {
    this.update.reset();

    // Tiefe Kopie der Liste.
    const newProfile: profileContract.IUserProfileContract = JSON.parse(
      JSON.stringify(this.application.profile)
    );

    // Binden.
    this.maxFavorites.data = newProfile;
    this.backToGuide.data = newProfile;
    this.encryption.data = newProfile;
    this.guideRows.data = newProfile;
    this.postGuide.data = newProfile;
    this.preGuide.data = newProfile;
    this.planDays.data = newProfile;
    this.allAudio.data = newProfile;
    this.dolby.data = newProfile;
    this.sourceType.data = newProfile;
    this.subs.data = newProfile;
    this.ttx.data = newProfile;

    // Die Anwendung wird nun zur Bedienung freigegeben.
    this.application.isBusy = false;
  }

  // Die Überschrift für die Anzeige des Präsentationsmodells.
  get title(): string {
    return "Individuelle Einstellungen ändern";
  }

  // Gesetzt wenn alle Eingaben konsistenz sind.
  private get isValid(): boolean {
    if (this.planDays.message) return false;
    if (this.guideRows.message) return false;
    if (this.preGuide.message) return false;
    if (this.postGuide.message) return false;
    if (this.maxFavorites.message) return false;

    return true;
  }

  // Stößt die Aktualisierung der Einstellungen.
  private save(): Promise<void> {
    // Nach dem erfolgreichen Speichern geht es mit der Einstiegsseite los, dabei werden die Einstellungen immer ganz neu geladen.
    return profileContract
      .setUserProfile(this.planDays.data)
      .then(() => this.application.gotoPage(null));
  }
}
