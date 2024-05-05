import * as React from 'react'

import { IDevicesPage } from '../../app/pages/devices'
import { IDeviceController } from '../../app/pages/devices/controller'
import { HelpLink } from '../../common/helpLink'
import { ButtonCommand } from '../../lib.react/command/button'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { EditNumberSlider } from '../../lib.react/edit/number/slider'
import { ComponentExWithSite, IComponent } from '../../lib.react/reactUi'

// Konfiguration der Steuerung einer laufenden Aufzeichnung.
interface IDeviceControl extends IComponent<IDeviceController> {
    // Der zugehörige Navigationsbereich.
    page: IDevicesPage
}

// React.Js Komponente zur Steuerung einer laufenden Aufzeichnung.
export class DeviceControl extends ComponentExWithSite<IDeviceController, IDeviceControl> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <fieldset className='vcrnet-device-control'>
                {this.props.uvm.live && (
                    <div>
                        <a href={this.props.uvm.live}>Aktuelle Aufzeichnung anschauen</a>
                    </div>
                )}
                {this.props.uvm.timeshift && (
                    <div>
                        <a href={this.props.uvm.timeshift}>Aufzeichnung zeitversetzt anschauen</a>
                    </div>
                )}
                {this.props.uvm.target && (
                    <div className='vcrnet-device-target'>
                        Aufzeichnung wird aktuell versendet, Empfänger ist {this.props.uvm.target}
                        <HelpLink page={this.props.page} topic='streaming' />
                    </div>
                )}
                <table className='vcrnet-tableIsForm'>
                    <tbody>
                        <tr>
                            <td>Endzeitpunkt:</td>
                            <td>{this.props.uvm.end}</td>
                        </tr>
                        <tr>
                            <td>Verbleibende Dauer:</td>
                            <td>{`${this.props.uvm.remaining.value} Minute${this.props.uvm.remaining.value === 1 ? '' : 'n'}`}</td>
                        </tr>
                    </tbody>
                </table>
                <EditNumberSlider uvm={this.props.uvm.remaining} />
                <div>
                    <ButtonCommand uvm={this.props.uvm.stopNow} />
                    <ButtonCommand uvm={this.props.uvm.update} />
                    <EditBoolean uvm={this.props.uvm.noHibernate} />
                    <HelpLink page={this.props.page} topic='hibernation' />
                </div>
            </fieldset>
        )
    }
}
