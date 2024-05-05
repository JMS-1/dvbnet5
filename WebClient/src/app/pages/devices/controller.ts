import { Command, ICommand } from '../../../lib/command/command'
import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { INumberWithSlider, NumberWithSlider } from '../../../lib/edit/number/slider'
import { IConnectable, IView } from '../../../lib/site'
import { IPlanCurrentContract, updateEndTime } from '../../../web/IPlanCurrentContract'
import { getDeviceRoot } from '../../../web/VCRServer'

// Schnittstelle zur Anzeige und Manipulation einer Aktivität.
export interface IDeviceController extends IConnectable {
    // Aktueller Endzeitpunkt.
    readonly end: string

    // Einstellung für die verbleibende Restzeit.
    readonly remaining: INumberWithSlider

    // Verweis zur LIVE Betrachtung.
    readonly live: string

    // Verweis zur Zeitversetzen Betrachtung.
    readonly timeshift: string

    // Aktueller Ziel für den Netzwerkversand.
    readonly target: string

    // Befehl um das sofortige Beenden vorzubereiten.
    readonly stopNow: ICommand

    // Einstellung zum Umgang mit dem Schlafzustand beim Vorzeitigen beenden.
    readonly noHibernate: IFlag

    // Befehl zur Aktualisierung der Endzeit.
    readonly update: ICommand
}

// Präsentationsmodell zur Ansicht und Pflege einer laufenden Aufzeichnung.
export class Controller implements IDeviceController {
    // Aktuell verbundenes Oberflächenelement.
    view: IView

    // Einstellung für die verbleibende Restzeit.
    readonly remaining = new NumberWithSlider({}, 'value', () => this.refreshUi(), 0, 480)

    // Befehl um das sofortige Beenden vorzubereiten.
    readonly stopNow = new Command(
        () => this.remaining.sync(0),
        'Vorzeitig beenden',
        () => this.remaining.value !== 0
    )

    // Einstellung zum Umgang mit dem Schlafzustand beim Vorzeitigen beenden.
    readonly noHibernate = new BooleanProperty({}, 'value', 'Übergang in den Schlafzustand unterdrücken')

    // Befehl zur Aktualisierung der Endzeit.
    readonly update = new Command(() => this.save(), 'Übernehmen')

    // Verweis zur LIVE Betrachtung.
    readonly live: string

    // Verweis zur Zeitversetzen Betrachtung.
    readonly timeshift: string

    constructor(
        private readonly _model: IPlanCurrentContract,
        suppressHibernate: boolean,
        private readonly _reload: () => void
    ) {
        // Präsentationsmodelle aufsetzen.
        this.remaining.value = _model.remainingTimeInMinutes
        this.noHibernate.value = suppressHibernate

        // Nur wenn es sich um eine Aufzeichnung handelt müssen wir mehr tun - Sonderaufgaben kann man nicht ansehen!
        if (_model.index < 0) return

        // Verweise zur Ansicht mit dem DVB.NET / VCR.NET Viewer aufsetzen.
        const url = `${getDeviceRoot()}${encodeURIComponent(_model.profileName)}/${_model.index}/`

        this.live = `${url}Live`
        this.timeshift = `${url}TimeShift`
    }

    // Start der Aufzeichnung.
    private get start(): Date {
        return new Date(this._model.startTimeISO)
    }

    // Das aktuelle Ende der Aufzeichnung.
    private get currentEnd(): Date {
        return new Date(
            this.start.getTime() +
                1000 * this._model.durationInSeconds +
                60000 * ((this.remaining.value ?? 0) - this._model.remainingTimeInMinutes)
        )
    }

    // Aktueller Endzeitpunkt formatiert als Uhrzeit.
    get end(): string {
        return DateTimeUtils.formatEndTime(this.currentEnd)
    }

    // Aktueller Ziel für den Netzwerkversand.
    get target(): string {
        return this._model.streamTarget
    }

    // Fordert die Oberfläche zur Aktualisierung auf.
    private refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }

    // Aktualisiert den Endzeitpunkt.
    private save(): Promise<void> {
        // Beim vorzeitigen Beenden sind wir etwas übervorsichtig.
        const end = (this.remaining.value ?? 0) > 0 ? this.currentEnd : this.start

        // Asynchronen Aufruf absetzen und im Erfolgsfall die Aktivitäten neu laden.
        return updateEndTime(this._model.profileName, !!this.noHibernate.value, this._model.planIdentifier, end).then(
            () => this._reload()
        )
    }
}
