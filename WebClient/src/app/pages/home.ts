import { IPage, Page } from './page'

import { Command, ICommand } from '../../lib/command/command'
import { BooleanProperty, IFlag } from '../../lib/edit/boolean/flag'
import { triggerTask } from '../../web/IPlanCurrentContract'
import { doUrlCall } from '../../web/VCRServer'
import { Application } from '../app'

// Schnittstelle zur Anzeige der Startseite.
export interface IHomePage extends IPage {
    // Befehl zum Starten der Aktualisierung der Programmzeichschrift.
    readonly startGuide: ICommand

    // Umschalter zur Anzeige des Bereichs zum Starten der Programmzeitschrift.
    readonly showStartGuide: IFlag

    // Befehl zum Starten eines Sendersuchlaufs.
    readonly startScan: ICommand

    // Umschalter zur Anzeige des Bereichs zum Starten eines Sendersuchlaufs.
    readonly showStartScan: IFlag

    // Gesetzt solange irgendeine Aufzeichnung auf irgendeinem gerät aktiv ist.
    readonly isRecording: boolean

    // Umschalter zur prüfung der Online verfügbaren Version des VCR.NET Recording Service.
    readonly checkVersion: IFlag

    // Die aktuell installierte Version.
    readonly currentVersion: string

    // Die Online verfügbare Version.
    readonly onlineVersion: string

    // Gesetzt, wenn die Online verfügbare Version anders ist als die lokal installierte - im Allgemeinen natürlich neuer.
    readonly versionMismatch: boolean

    // Gesetzt, wenn der Verweis zur Konfiguration angezeigt werden soll.
    readonly showAdmin: boolean
}

// Die Anwendungslogik für die Startseite.
export class HomePage extends Page implements IHomePage {
    // Ausdruck zur Extraktion der Online Version aus der Verzeichnisliste des Downloadsbereichs.
    private static _versionExtract = />VCRNET\.MSI<\/a>[^<]*\s([^\s]+)\s*</i

    // Befehl zum Starten der Aktualisierung der Programmzeichschrift.
    readonly startGuide = new Command(
        () => this.startTask('guide'),
        'Aktualisierung anfordern',
        () => this.application.version.guideUpdateEnabled
    )

    // Umschalter zur Anzeige des bereichs zum Starten der Programmzeitschrift.
    readonly showStartGuide = new BooleanProperty(
        {} as { value?: boolean },
        'value',
        'die Programmzeitschrift sobald wie möglich aktualisieren',
        () => this.refreshUi(),
        () => !this.application.version.guideUpdateEnabled
    )

    // Befehl zum Starten eines Sendersuchlaufs.
    readonly startScan = new Command(
        () => this.startTask('scan'),
        'Aktualisierung anfordern',
        () => this.application.version.sourceScanEnabled
    )

    // Umschalter zur Anzeige des Bereichs zum Starten eines Sendersuchlaufs.
    readonly showStartScan = new BooleanProperty(
        {} as { value?: boolean },
        'value',
        'einen Sendersuchlauf sobald wie möglich durchführen',
        () => this.refreshUi(),
        () => !this.application.version.sourceScanEnabled
    )

    // Umschalter zur prüfung der Online verfügbaren Version des VCR.NET Recording Service.
    readonly checkVersion = new BooleanProperty({} as { value?: string }, 'value', 'neue Version', () =>
        this.toggleVersionCheck()
    )

    // Erstellt die Anwendungslogik.
    constructor(application: Application) {
        super('home', application)

        // Meldet, dass die Navigationsleiste nicht angezeigt werden soll.
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        this.navigation = null!
    }

    // Die aktuell installierte Version.
    get currentVersion(): string {
        return this.application.version.installedVersion
    }

    // Die Online verfügbare Version.
    private _onlineVersion?: string

    get onlineVersion(): string {
        return this._onlineVersion || '(wird ermittelt)'
    }

    // Gesetzt, wenn die Online verfügbare Version anders ist als die lokal installierte - im Allgemeinen natürlich neuer.
    get versionMismatch(): boolean {
        return !!this._onlineVersion && this._onlineVersion !== this.currentVersion
    }

    // Gesetzt, wenn der Verweis zur Konfiguration angezeigt werden soll.
    get showAdmin(): boolean {
        return this.application.version.isAdmin && !this.isRecording
    }

    // Blendet die Anzeile der Online Version ein oder aus.
    private toggleVersionCheck(): void {
        // Beim Einblenden wird die aktuelle Online Version immer neu angefordert.
        if (this.checkVersion.value) {
            this._onlineVersion = undefined

            // Downloadverzeichnis abrufen.
            doUrlCall('http://downloads.psimarron.net').then((html: string) => {
                // Versionsnummer extrahieren.
                const match = HomePage._versionExtract.exec(html)
                if (match == null) return
                if (match.length < 2) return

                // Version vermerken.
                this._onlineVersion = match[1]

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi()
            })
        }

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // Zeigt die Startseite (erneut) an.
    reset(sections: string[]): void {
        // Initialen Stand wieder herstellen.
        this.startScan.reset()
        this.startGuide.reset()
        this.checkVersion.value = false
        this.showStartScan.value = false
        this.showStartGuide.value = false

        this._onlineVersion = undefined

        // Anwendung zur Bedienung freigeben.
        this.application.isBusy = false
    }

    // Meldet die Überschrift der Startseite.
    get title(): string {
        // Der Titel wird dem aktuellen Kenntnisstand angepasst.
        const version = this.application.version
        const title = this.application.title

        if (version) return `${title} (${version.installedVersion})`
        else return title
    }

    // Gesetzt solange irgendeine Aufzeichnung auf irgendeinem gerät aktiv ist.
    get isRecording(): boolean {
        return this.application.version.isRunning
    }

    // Aktiviert eine Sonderaufgabe und wechselt im Erfolgsfall nach Bestätigung durch den VCR.NET Recording Service auf die Geräteübersicht.
    private startTask(task: 'guide' | 'scan'): Promise<void> {
        return triggerTask(task).then(() => this.application.gotoPage(this.application.devicesPage.route))
    }
}
