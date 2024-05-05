import * as React from 'react'

import { AdminSection } from './section'

import { DevicesSection, IAdminDevicesPage } from '../../app/pages/admin/devices'
import { Field } from '../../common/field'
import { HelpLink } from '../../common/helpLink'
import { InlineHelp } from '../../common/inlineHelp'
import { ExternalLink } from '../../lib.react/command/externalLink'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { SingleSelect } from '../../lib.react/edit/list'
import { EditNumber } from '../../lib.react/edit/number/number'
import { IAdminSectionFactory } from '../admin'

// React.js Komponente zur Konfiguration der Geräte.
export class AdminDevices extends AdminSection<IAdminDevicesPage> {
    // Das zugehörige Ui View Model.
    static get uvm(): IAdminSectionFactory<IAdminDevicesPage> {
        return DevicesSection
    }

    // Die Überschrift für diesen Bereich.
    protected readonly title = 'Aktivierung von DVB.NET Geräteprofilen'

    // Oberflächenelemente anlegen.
    protected renderSection(): JSX.Element {
        return (
            <div className='vcrnet-admin-devices'>
                Für den VCR.NET Recording Service kann festgelegt werden, welche der auf dem zugehörigen Rechner
                installierten DVB.NET
                <HelpLink page={this.props.uvm.page} topic='dvbnet' /> Geräteprofile für Aufzeichnungen verwendet werden
                dürfen. Eines dieser Geräte muss dann als bevorzugtes Gerät festgelegt werden.
                {this.getHelp()}
                <form>
                    <Field label={`${this.props.uvm.defaultDevice.text}:`} page={this.props.uvm.page}>
                        <SingleSelect uvm={this.props.uvm.defaultDevice} />
                    </Field>
                    <table className='vcrnet-table'>
                        <thead>
                            <tr>
                                <td>Verwenden</td>
                                <td>Name</td>
                                <td>Priorität</td>
                                <td>Entschlüsselung</td>
                                <td>Quellen</td>
                            </tr>
                        </thead>
                        <tbody>
                            {this.props.uvm.devices.map((d) => (
                                <tr key={d.name}>
                                    <td>
                                        <EditBoolean uvm={d.active} />
                                    </td>
                                    <td>{d.name}</td>
                                    <td>
                                        <EditNumber chars={5} uvm={d.priority} />
                                    </td>
                                    <td>
                                        <EditNumber chars={5} uvm={d.decryption} />
                                    </td>
                                    <td>
                                        <EditNumber chars={5} uvm={d.sources} />
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </form>
            </div>
        )
    }

    // Allgemeine Erläuterungen.
    private getHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Die Tabelle zeigt alle DVB.NET Geräte des Rechners, auf dem der VCR.NET Dienst läuft. Über die erste
                Spalte können beliebig viele davon für Aufzeichnungen durch den VCR.NET Recording Service freigeschaltet
                werden. Eines dieser Geräte muss dann zusätzlich als bevorzugtes Gerät festgelegt werden. Dieses
                bevorzugte Gerät wird zum Beispiel bei neuen Aufträgen aber auch beim Aufruf der Programmzeitschrift als
                Vorauswahl verwendet.
                <br />
                <br />
                Zusätzlich können für jedes Gerät auch einige Geräteparameter festgelegt werden - alternativ zur
                direkten Pflege über die{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNET/html/dialogrecording.html'>
                    DVB.NET Konfiguration und Administration
                </ExternalLink>
                . Hier vorgenomme Änderungen werden für alle Geräte übernommen, selbst wenn VCR.NET diese nicht
                verwendet. Grundsätzlich werden Änderungen in der Tabelle erst durch eine explizite Bestätigung über die
                entsprechende Schaltfläche übernommen. Änderungen an den Geräten erfordern fast immer einen Neustart des
                VCR.NET Dienstes.
            </InlineHelp>
        )
    }
}
