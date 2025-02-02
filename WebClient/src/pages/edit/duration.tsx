import * as React from 'react'

import { IDurationEditor } from '../../app/pages/edit/duration'
import { EditTime } from '../../lib.react/edit/datetime/time'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Auswahl der Dauer eine Aufzeichnung über die EIngabe von Start- und Endzeit.
export class EditDuration extends Component<IDurationEditor> {
    // Oberflächenelemente anlegen.
    render(): React.JSX.Element {
        return (
            <div className='vcrnet-editduration'>
                <EditTime uvm={this.props.uvm.startTime} /> bis <EditTime uvm={this.props.uvm.endTime} /> Uhr
            </div>
        )
    }
}
