import * as React from 'react'

import { Navigation } from './navigation'
import { View } from './view'

import { Application, IApplication, IApplicationSite } from '../app/app'
import { IHelpComponent, IHelpComponentProvider } from '../app/pages/help'
import { IEmpty } from '../lib.react/reactUi'
import { AdminProgramGuide } from '../pages/help/adminGuide'
import { AdminSourceScan } from '../pages/help/adminScan'
import { Archive } from '../pages/help/archive'
import { Configuration } from '../pages/help/configuration'
import { CurrentStream } from '../pages/help/currentStream'
import { CustomSchedule } from '../pages/help/customSchedule'
import { Decryption } from '../pages/help/decryption'
import { DvbNet } from '../pages/help/dvbNet'
import { EditCurrent } from '../pages/help/editCurrent'
import { ProgramGuide } from '../pages/help/epg'
import { FileContents } from '../pages/help/fileContents'
import { Overview } from '../pages/help/home'
import { JobsAndSchedules } from '../pages/help/jobsAndSchedules'
import { Log } from '../pages/help/log'
import { NumberOfFiles } from '../pages/help/numberOfFiles'
import { ParallelRecording } from '../pages/help/parallelRecording'
import { RepeatingSchedules } from '../pages/help/repeatingSchedules'
import { SourceChooser } from '../pages/help/sourceChooser'
import { Streaming } from '../pages/help/streaming'
import { Tasks } from '../pages/help/tasks'
import { TsPlayer } from '../pages/help/tsPlayer'

// React.Js Komponente für die Hauptseite der Anwendung - im Prinzip der gesamte sichtbare Bereich im Browser.
export class Main extends React.Component<IEmpty, IEmpty> implements IApplicationSite {
    // Alle bekannten Hilfeseiten.
    private readonly _topics: { [section: string]: IHelpComponent } = {
        archive: new Archive(),
        configuration: new Configuration(),
        currentstream: new CurrentStream(),
        customschedule: new CustomSchedule(),
        decryption: new Decryption(),
        dvbnet: new DvbNet(),
        editcurrent: new EditCurrent(),
        epg: new ProgramGuide(),
        epgconfig: new AdminProgramGuide(),
        filecontents: new FileContents(),
        jobsandschedules: new JobsAndSchedules(),
        log: new Log(),
        numberoffiles: new NumberOfFiles(),
        overview: new Overview(),
        parallelrecording: new ParallelRecording(),
        psiconfig: new AdminSourceScan(),
        repeatingschedules: new RepeatingSchedules(),
        sourcechooser: new SourceChooser(),
        streaming: new Streaming(),
        tasks: new Tasks(),
        tsplayer: new TsPlayer(),
    }

    // Das Präsentationsmodell der Anwendung.
    private readonly _application: IApplication = new Application(this)

    // Wird ausgelöst, wenn sich der Navigationsberich ändert.
    private readonly _onhashchange: () => void = this.onhashchange.bind(this)

    // Erstellt eine neue Komponente.
    constructor(props: IEmpty) {
        super(props)

        // Initialen Navigationsbereich in Abhängigkeit von der URL aufrufen.
        this.onhashchange()
    }

    // Anmeldung beim Anbinden der React.Js Komponente ins DOM.
    private _mounted = false

    componentDidMount(): void {
        this._mounted = true

        window.addEventListener('hashchange', this._onhashchange)
    }

    // Abmelden beim Entfernen aus dem DOM - tatsächlich passiert dies nie.
    componentWillUnmount(): void {
        window.removeEventListener('hashchange', this._onhashchange)
    }

    // React.Js zur Aktualisierung der Oberfläche auffordern.
    refreshUi(): void {
        if (this._mounted) this.forceUpdate()
    }

    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        // Überschrift ermitteln.
        const title = this._application.title
        const page = this._application.page

        if (document.title !== title) document.title = title

        // Anzeige erstellen.
        return (
            <div className='vcrnet-main'>
                {this._application.isRestarting ? (
                    <div className='vcrnet-restart'>
                        Der VCR.NET Recording Service startet nun neu und steht in Kürze wieder zur Verfügung.
                    </div>
                ) : page ? (
                    <div>
                        <h1>{page ? page.title : title}</h1>
                        <Navigation uvm={page} />
                        <View uvm={page} />
                    </div>
                ) : (
                    <div>
                        <h1>(Bitte etwas Geduld)</h1>
                    </div>
                )}
                {this._application.isBusy && <div className='vcrnet-main-busy'></div>}
            </div>
        )
    }

    // Wird zur Aktualisierung des Navigationsbereichs aufgerufen.
    private onhashchange(): void {
        // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
        const query = window.location.href.split('#')
        const hash = query.length > 1 ? query[1] : ''

        // Erst mal auf die Einstiegsseite prüfen.
        if (hash.length < 1) this.setPage()
        else {
            // Ansonsten den Navigationsbereich mit Parametern aufrufen.
            const sections = hash.split(';')

            this.setPage(sections[0], sections.slice(1))
        }
    }

    // Den Navigationsbereich wechseln.
    private setPage(name = '', sections?: string[]) {
        this._application.switchPage(name, sections)
    }

    // Den Navigationsberecich über den Browser ändern.
    goto(name: string): void {
        window.location.href = name ? `#${name}` : '#'
    }

    // Die Verwaltung der Hilfeseiten melden.
    getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType> {
        return this._topics as IHelpComponentProvider<TComponentType>
    }
}
