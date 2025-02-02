import * as React from 'react'

import { AdminSection } from './section'

import { IAdminOtherPage, OtherSection } from '../../app/pages/admin/other'
import { Field } from '../../common/field'
import { HelpLink } from '../../common/helpLink'
import { InlineHelp } from '../../common/inlineHelp'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { SingleSelect } from '../../lib.react/edit/list'
import { EditNumber } from '../../lib.react/edit/number/number'
import { IAdminSectionFactory } from '../admin'

// React.Js Komponente zur Konfiguration sonstiger Einstellungen.
export class AdminOther extends AdminSection<IAdminOtherPage> {
    // Das zugehörige Ui View Model.
    static get uvm(): IAdminSectionFactory<IAdminOtherPage> {
        return OtherSection
    }

    // Die Überschrift für diesen Bereich.
    protected readonly title = 'Sonstige Betriebsparameter'

    // Erstellt die Oberflächenelemente.
    protected renderSection(): React.JSX.Element {
        return (
            <div className='vcrnet-admin-other'>
                Hier handelt es sich um grundsätzliche Betriebsparameter des VCR.NET Dienstes. Änderungen hier erfordern
                üblicherweise ein tieferes Verständnis der Arbeitsweise des VCR.NET Recording Service, da
                Fehleinstellungen durchaus dazu führen können, dass der Dienst nicht mehr funktionsfähig ist.
                <form>
                    {this.getLogHelp()}
                    <Field label={`${this.props.uvm.logKeep.text}:`} page={this.props.uvm.page}>
                        <EditNumber chars={8} uvm={this.props.uvm.logKeep} />
                    </Field>
                    <Field label={`${this.props.uvm.jobKeep.text}:`} page={this.props.uvm.page}>
                        <EditNumber chars={8} uvm={this.props.uvm.jobKeep} />
                    </Field>
                    <div>
                        <EditBoolean uvm={this.props.uvm.noH264PCR} />
                    </div>
                    <div>
                        <EditBoolean uvm={this.props.uvm.noMPEG2PCR} />
                    </div>
                    <Field label={`${this.props.uvm.logging.text}:`} page={this.props.uvm.page}>
                        <SingleSelect uvm={this.props.uvm.logging} />
                    </Field>
                </form>
                {this.getSaveHelp()}
            </div>
        )
    }

    // Hilfe zur Protokollierung.
    private getLogHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Nach erfolgter Aufzeichnung erstellt der VCR.NET Recording Service automatisch einen Protokolleintrag
                mit den Eckdaten der Gerätenutzung.
                <HelpLink page={this.props.uvm.page} topic='log' /> Diese werden allerdings nur eine begrenzte Zeit
                vorgehalten und dann automatisch endgültig gelöscht. Ähnlich verhält es sich mit vollständig
                abgeschlossenen Aufträgen:
                <HelpLink page={this.props.uvm.page} topic='archive' /> diese werden für einen gewissen Zeitraum
                archiviert, bevor sie endgültig entfernt werden. Während des Verbleibs im Archive können sie jederzeit
                abgefrufen
                <HelpLink page={this.props.uvm.page} topic='archive' /> und erneut verwendet werden.
            </InlineHelp>
        )
    }

    // Hilfe zum Speichern der Konfiguration.
    private getSaveHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Alle Änderungen werden erst durch explizites Betätigen der Schaltfläche übernommen. Einige Änderungen
                wie etwa die Konfiguration des Web Servers erfordern den Neustart des VCR.NET Dienstes.
            </InlineHelp>
        )
    }
}
