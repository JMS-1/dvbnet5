﻿import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IString, StringProperty } from '../../../lib/edit/text/text'
import { IDisplay } from '../../../lib/localizable'
import { IEditJobScheduleCommonContract } from '../../../web/IEditJobScheduleCommonContract'
import { ChannelProperty, IChannelSelector } from '../channel'
import { IPage } from '../page'

// Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
export interface ISourceFlagsEditor extends IDisplay {
    // Gesetzt um alle Sprachen aufzuzeichnen
    readonly allLanguages: IFlag

    // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
    readonly includeDolby: IFlag

    // Gesetzt, um den Videotext aufzuzeichnen
    readonly withVideotext: IFlag

    // Gesetzt, um die Untertitel aufzuzeichnen
    readonly withSubtitles: IFlag
}

// Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
export interface IJobScheduleEditor {
    // Die zugehörige Seite der Anwendung.
    readonly page: IPage

    // Der Name des Auftrags.
    readonly name: IString

    // Der Name der Quelle, die aufgezeichnet werden soll.
    readonly source: IChannelSelector

    // Aufzeichnungsoptionen.
    readonly sourceFlags: ISourceFlagsEditor
}

// Bietet die gemeinsamen Daten eines Auftrags oder einer Aufzeichnung zur Pflege an.
export abstract class JobScheduleEditor<TModelType extends IEditJobScheduleCommonContract>
    implements IJobScheduleEditor
{
    // Ein Muster zum Erkennen gültiger Namen von Aufzeichnungen.
    private static readonly _allowedCharacters = /^[^\\/:*?"<>|]*$/

    // Erstelltein neues Präsentationsmodell an.
    constructor(
        public readonly page: IPage,
        protected model: TModelType,
        favoriteSources: string[],
        onChange: () => void
    ) {
        // Prüfung auf die Auswahl einer Quelle - ohne eine solche machen die Optionen zur Aufzeichnung auch keinen Sinn.
        const noSource = (): boolean => (this.source.value || '').trim().length < 1

        // Pflegekomponenten erstellen
        this.name = new StringProperty(this.model, 'name', 'Name', onChange)
        this.source = new ChannelProperty(page.application.profile, this.model, 'source', favoriteSources, onChange)
        this.sourceFlags = {
            allLanguages: new BooleanProperty(this.model, 'allLanguages', 'Alle Sprachen', onChange, noSource),
            includeDolby: new BooleanProperty(this.model, 'dolbyDigital', 'Dolby Digital (AC3)', onChange, noSource),
            text: 'Besonderheiten',
            withSubtitles: new BooleanProperty(this.model, 'dvbSubtitles', 'DVB Untertitel', onChange, noSource),
            withVideotext: new BooleanProperty(this.model, 'videotext', 'Videotext', onChange, noSource),
        }

        // Zusätzliche Prüfungen einrichten.
        this.name.addPatternValidator(JobScheduleEditor._allowedCharacters, 'Der Name enthält ungültige Zeichen')

        // Initiale Prüfung.
        this.name.validate()
        this.sourceFlags.includeDolby.validate()
        this.sourceFlags.allLanguages.validate()
        this.sourceFlags.withSubtitles.validate()
        this.sourceFlags.withVideotext.validate()
    }

    // Der Name des Auftrags.
    readonly name: StringProperty<TModelType>

    // Der Name der Quelle, die aufgezeichnet werden soll.
    readonly source: ChannelProperty<TModelType>

    // Aufzeichnungsoptionen.
    readonly sourceFlags: {
        readonly text: string
        readonly allLanguages: BooleanProperty<TModelType>
        readonly includeDolby: BooleanProperty<TModelType>
        readonly withVideotext: BooleanProperty<TModelType>
        readonly withSubtitles: BooleanProperty<TModelType>
    }

    // Gesetzt, wenn die Einstellungen der Quelle gültig sind.
    isValid(): boolean {
        // Wir fragen einfach alle unsere Präsentationsmodelle.
        if (this.name.message) return false
        if (this.source.sourceName.message) return false
        if (this.sourceFlags.allLanguages.message) return false
        if (this.sourceFlags.includeDolby.message) return false
        if (this.sourceFlags.withVideotext.message) return false
        if (this.sourceFlags.withSubtitles.message) return false

        return true
    }
}
