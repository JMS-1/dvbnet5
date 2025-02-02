import * as React from 'react'

import { IDeviceInfo } from '../../app/pages/devices/entry'
import { ComponentWithSite } from '../../lib.react/reactUi'
import { TimeBar } from '../../lib.react/timeBar'
import { GuideEntryInfo } from '../guide/info'

// React.Js Komponente zur Anzeige des Auszugs der Programmzeitschrift für eine Aktivität.
export class DeviceGuide extends ComponentWithSite<IDeviceInfo> {
    // Oberflächenelemente erstellen.
    render(): React.JSX.Element {
        return (
            <fieldset className='vcrnet-device-epg'>
                {this.props.uvm.guideTime && <TimeBar uvm={this.props.uvm.guideTime} />}
                {this.props.uvm.guideItem && <GuideEntryInfo uvm={this.props.uvm.guideItem} />}
            </fieldset>
        )
    }
}
