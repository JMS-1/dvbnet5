import * as React from 'react'

import { IDevicesPage } from '../../app/pages/devices'
import { IDeviceInfo } from '../../app/pages/devices/entry'
import { InternalLink } from '../../lib.react/command/internalLink'
import { Pictogram } from '../../lib.react/command/pictogram'
import { ComponentEx, IComponent } from '../../lib.react/reactUi'

// Konfiguration zur Anzeige einer einzelnen Aktivität.
interface IDevice extends IComponent<IDeviceInfo> {
    // Der zugehörige Navigationsbereich.
    page: IDevicesPage
}

// React.Js Komponente zur Anzeige einer Aktivität.
export class Device extends ComponentEx<IDeviceInfo, IDevice> {
    // Erstellt die Oberflächenelemente.
    render(): JSX.Element {
        const { showGuide, showControl, mode, liveUri, demux } = this.props.uvm
        const done = mode === 'done'

        return (
            <tr className='vcrnet-device'>
                <td>
                    {done ? (
                        <a href={demux}>
                            <Pictogram name={mode} />
                        </a>
                    ) : mode ? (
                        showControl.isReadonly ? (
                            <Pictogram name={mode} />
                        ) : (
                            <InternalLink view={() => (showControl.value = !showControl.value)}>
                                <Pictogram name={mode} />
                            </InternalLink>
                        )
                    ) : (
                        <span>&nbsp;</span>
                    )}
                </td>
                <td>
                    {showGuide.isReadonly ? (
                        <span>{this.props.uvm.displayStart}</span>
                    ) : (
                        <InternalLink view={() => (showGuide.value = !showGuide.value)}>
                            {this.props.uvm.displayStart}
                        </InternalLink>
                    )}
                </td>
                <td>{done ? '' : this.props.uvm.displayEnd}</td>
                <td>{done ? '' : this.props.uvm.source}</td>
                <td>
                    {this.props.uvm.id ? (
                        <InternalLink view={`${this.props.page.application.editPage.route};id=${this.props.uvm.id}`}>
                            {this.props.uvm.name}
                        </InternalLink>
                    ) : (
                        <span>{this.props.uvm.name}</span>
                    )}
                </td>
                <td>
                    <div className='device-name'>
                        <a className={mode === 'running' || mode === 'done' ? 'inactive' : ''} href={liveUri}>
                            <Pictogram name='running' />
                        </a>
                        <span>{this.props.uvm.device}</span>
                    </div>
                </td>
                <td>{this.props.uvm.size}</td>
            </tr>
        )
    }
}
