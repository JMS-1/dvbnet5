import * as React from 'react'
import { IApplicationSite, IApplication, Application } from '../app/app'
import { IHelpComponent, IHelpComponentProvider } from '../app/pages/help'
import { IEmpty } from '../lib.react/reactUi'
import { Navigation } from './navigation'
import { View } from './view'
import { AdminProgramGuide } from '../pages/help/adminGuide'
import { AdminSourceScan } from '../pages/help/adminScan'
import { Archive } from '../pages/help/archive'
import { Configuration } from '../pages/help/configuration'
import { ControlCenter } from '../pages/help/controlCenter'
import { CurrentStream } from '../pages/help/currentStream'
import { CustomSchedule } from '../pages/help/customSchedule'
import { Decryption } from '../pages/help/decryption'
import { DvbNet } from '../pages/help/dvbNet'
import { EditCurrent } from '../pages/help/editCurrent'
import { ProgramGuide } from '../pages/help/epg'
import { FileContents } from '../pages/help/fileContents'
import { Hibernation } from '../pages/help/hibernation'
import { Overview } from '../pages/help/home'
import { JobsAndSchedules } from '../pages/help/jobsAndSchedules'
import { Nexus } from '../pages/help/nexus'
import { NumberOfFiles } from '../pages/help/numberOfFiles'
import { ParallelRecording } from '../pages/help/parallelRecording'
import { RepeatingSchedules } from '../pages/help/repeatingSchedules'
import { SourceChooser } from '../pages/help/sourceChooser'
import { SourceLimit } from '../pages/help/sourceLimit'
import { Streaming } from '../pages/help/streaming'
import { Tasks } from '../pages/help/tasks'
import { TsPlayer } from '../pages/help/tsPlayer'
import { WebSettings } from '../pages/help/webSettings'
import { Log } from '../pages/help/log'

// React.Js Komponente für die Hauptseite der Anwendung - im Prinzip der gesamte sichtbare Bereich im Browser.
export class Main extends React.Component<IEmpty, IEmpty> implements IApplicationSite {
    // Alle bekannten Hilfeseiten.
    private readonly _topics: { [section: string]: IHelpComponent } = {
        repeatingschedules: new RepeatingSchedules(),
        parallelrecording: new ParallelRecording(),
        jobsandschedules: new JobsAndSchedules(),
        customschedule: new CustomSchedule(),
        configuration: new Configuration(),
        controlcenter: new ControlCenter(),
        currentstream: new CurrentStream(),
        epgconfig: new AdminProgramGuide(),
        numberoffiles: new NumberOfFiles(),
        sourcechooser: new SourceChooser(),
        filecontents: new FileContents(),
        psiconfig: new AdminSourceScan(),
        editcurrent: new EditCurrent(),
        hibernation: new Hibernation(),
        sourcelimit: new SourceLimit(),
        websettings: new WebSettings(),
        decryption: new Decryption(),
        streaming: new Streaming(),
        overview: new Overview(),
        tsplayer: new TsPlayer(),
        epg: new ProgramGuide(),
        archive: new Archive(),
        dvbnet: new DvbNet(),
        nexus: new Nexus(),
        tasks: new Tasks(),
        log: new Log(),
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
    componentDidMount(): void {
        window.addEventListener('hashchange', this._onhashchange)
    }

    // Abmelden beim Entfernen aus dem DOM - tatsächlich passiert dies nie.
    componentWillUnmount(): void {
        window.removeEventListener('hashchange', this._onhashchange)
    }

    // React.Js zur Aktualisierung der Oberfläche auffordern.
    refreshUi(): void {
        this.forceUpdate()
    }

    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        // Überschrift ermitteln.
        var title = this._application.title
        var page = this._application.page

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
        var query = window.location.href.split('#')
        var hash = query.length > 1 ? query[1] : ''

        // Erst mal auf die Einstiegsseite prüfen.
        if (hash.length < 1) this.setPage()
        else {
            // Ansonsten den Navigationsbereich mit Parametern aufrufen.
            var sections = hash.split(';')

            this.setPage(sections[0], sections.slice(1))
        }
    }

    // Den Navigationsbereich wechseln.
    private setPage(name: string = '', sections?: string[]) {
        this._application.switchPage(name, sections)
    }

    // Den Navigationsberecich über den Browser ändern.
    goto(name: string): void {
        window.location.href = name ? `#${name}` : `#`
    }

    // Die Verwaltung der Hilfeseiten melden.
    getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType> {
        return this._topics as IHelpComponentProvider<TComponentType>
    }
}
