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
        const showGuide = this.props.uvm.showGuide
        const showControl = this.props.uvm.showControl

        return (
            <tr className='vcrnet-device'>
                <td>
                    {this.props.uvm.mode ? (
                        showControl.isReadonly ? (
                            <Pictogram name={this.props.uvm.mode} />
                        ) : (
                            <InternalLink view={() => (showControl.value = !showControl.value)}>
                                <Pictogram name={this.props.uvm.mode} />
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
                <td>{this.props.uvm.displayEnd}</td>
                <td>{this.props.uvm.source}</td>
                <td>
                    {this.props.uvm.id ? (
                        <InternalLink view={`${this.props.page.application.editPage.route};id=${this.props.uvm.id}`}>
                            {this.props.uvm.name}
                        </InternalLink>
                    ) : (
                        <span>{this.props.uvm.name}</span>
                    )}
                </td>
                <td>{this.props.uvm.device}</td>
                <td>{this.props.uvm.size}</td>
            </tr>
        )
    }
}
