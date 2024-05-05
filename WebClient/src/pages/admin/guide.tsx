import * as React from 'react'

import { AdminSection } from './section'

import { GuideSection, IAdminGuidePage } from '../../app/pages/admin/guide'
import { Field } from '../../common/field'
import { HelpLink } from '../../common/helpLink'
import { InlineHelp } from '../../common/inlineHelp'
import { ButtonCommand } from '../../lib.react/command/button'
import { InternalLink } from '../../lib.react/command/internalLink'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { SingleSelect } from '../../lib.react/edit/list'
import { MultiSelectButton } from '../../lib.react/edit/multiButtonList'
import { MultiSelect } from '../../lib.react/edit/multiList'
import { EditNumber } from '../../lib.react/edit/number/number'
import { IAdminSectionFactory } from '../admin'
import { EditChannel } from '../edit/channel'

// React.Js Komponentezur Pflege der Konfiguration der Aktualisierung der Programmzeitschrift.
export class AdminGuide extends AdminSection<IAdminGuidePage> {
    // Das zugehörige Ui View Model.
    static get uvm(): IAdminSectionFactory<IAdminGuidePage> {
        return GuideSection
    }

    // Die Überschrift für diesen Bereich.
    protected readonly title = 'Einstellungen zum Aufbau der Programmzeitschrift'

    // Oberflächenelemente erzeugen
    protected renderSection(): JSX.Element {
        return (
            <div className='vcrnet-admin-guide'>
                <div>
                    Auf Wunsch kann der VCR.NET Recording Service die Elektronische Programmzeitschrift (EPG)
                    <HelpLink page={this.props.uvm.page} topic='epg' /> periodisch aktualisieren
                    <HelpLink page={this.props.uvm.page} topic='epgconfig' /> und dann zur Programmierung von neuen
                    Aufzeichnungen anbieten.
                    <InternalLink pict='new' view={this.props.uvm.page.application.editPage.route} /> Hier werden die
                    Eckdaten für die Aktualisierung festgelegt.
                </div>
                <EditBoolean uvm={this.props.uvm.isActive} />
                {this.props.uvm.isActive.value && (
                    <form>
                        {this.getSourceHelp()}
                        <div>
                            <MultiSelect items={10} uvm={this.props.uvm.sources} />
                            <ButtonCommand uvm={this.props.uvm.remove} />
                        </div>
                        <Field label={`${this.props.uvm.device.text}:`} page={this.props.uvm.page}>
                            <SingleSelect uvm={this.props.uvm.device} />
                            <EditChannel uvm={this.props.uvm.source} />
                            <ButtonCommand uvm={this.props.uvm.add} />
                        </Field>
                        <EditBoolean uvm={this.props.uvm.ukTv} />
                        {this.getDurationHelp()}
                        <Field label={`${this.props.uvm.duration.text}:`} page={this.props.uvm.page}>
                            <EditNumber chars={5} uvm={this.props.uvm.duration} />
                        </Field>
                        <Field label={`${this.props.uvm.hours.text}:`} page={this.props.uvm.page}>
                            <MultiSelectButton merge={true} uvm={this.props.uvm.hours} />
                        </Field>
                        <Field label={`${this.props.uvm.delay.text}:`} page={this.props.uvm.page}>
                            <EditNumber chars={5} uvm={this.props.uvm.delay} />
                        </Field>
                        <Field label={`${this.props.uvm.latency.text}:`} page={this.props.uvm.page}>
                            <EditNumber chars={5} uvm={this.props.uvm.latency} />
                        </Field>
                    </form>
                )}
                {this.getUpdateHelp()}
            </div>
        )
    }

    // Hilfe zum Speichern der Konfiguration.
    private getUpdateHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Alle Änderungen müssen durch das explizite Betätigen der entsprechenden Schaltfläche bestätigt werden
                und werden auch damit erst übernommen. Änderungen an der Konfiguration der Aktualisierung der
                Programmzeitschrift erfordern keinen Neustart des VCR.NET Dienstes.
            </InlineHelp>
        )
    }

    // Hilfe zur Auswahl der Quellen.
    private getSourceHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Der VCR.NET Recording Service verwaltet eine geräteübergreifende Liste von Quellen, die in der
                Programmzeitschrift zu berücksichtigen sind. Nach Auswahl von Quellen aus der Liste können diese einfach
                daraus entfernt werden.
                <br />
                <br />
                Sollen Quellen zur Liste hinzugeführt werden, so ist zuerst einmal das Gerät auszuwählen, über das die
                gewünschten Quellen empfangen werden können. Danach können die von der Programmierung neuer
                Aufzeichnungen her bekannten Mechanismen zur schnellen Auswahl der Quelle verwendet werden.
                <HelpLink page={this.props.uvm.page} topic='sourcechooser' />
                <br />
                <br />
                Wenn auch Quellen zu britischen Sendern in der Liste enthalten sind, so muss auch die Option aktiviert
                werden um die zugehörigen Vorschaudaten mit einzusammeln. Dies ist notwendig, da hierfür andere
                technische Voraussetzungen beim Emfpang der Elektronischen Programmzeitschrift gelten.
            </InlineHelp>
        )
    }

    // Hilfe zur Konfiguration der automatischen Aktualisierung.
    private getDurationHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Zur Planung der Aktualisierungen zwischen regulären Aufzeichnungen benötigt der VCR.NET Recording
                Service eine zeitliche Begrenzung für die Ausführung der Aktualisierung. Wird diese erreicht, so wird
                die Aktualisierung vorzeitig beendet und die Programmzeitschrift ist eventuell unvollständig. Zu große
                Werte können dazu führen, dass Aktualisierungen durch zeitliche Kollisionen mit regulären Aufzeichnungen
                später als geplant ausgeführt werden, da reguläre Aufzeichnung immer vorrangig ausgeführt werden. Ein
                realistischer Wert für die Laufzeit macht hier immer Sinn - kann die Aktualisierung in kürzerer Zeit
                abgeschlossen werden, so wird der VCR.NET Recording Service diese auch zeitnah beenden - das nutzt aber
                für die Aufzeichnungsplanung nichts.
                <br />
                <br />
                Für die Aktualisierungen werden oft eine Reihe fester Uhrzeiten vorgegeben. Ergänzend oder alternativ
                kann auch ein Zeitintervall vorgegeben werden, in dem Aktualisierungen durchgeführt werden sollen - etwa
                alle 4 Stunden. Eine Besonderheit ist die zusätzliche Latenzzeit: hat der VCR.NET Recording Service
                gerade eine Aufzeichnung ausgeführt und würde nun keines der Geräte mehr nutzen, so kann eine anstehende
                Aktualisierung vorzogen werden, wenn die vorherige Aktualisierung bereits mehr als das konfigurierte
                Intervall in der Vergangenheit liegt.
            </InlineHelp>
        )
    }
}
