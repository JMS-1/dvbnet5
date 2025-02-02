import * as React from 'react'

import { IScheduleException } from '../../app/pages/edit/exception'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Deaktivierung einer einzelnen Ausnahmeregel.
export class EditException extends Component<IScheduleException> {
    // Erstellt die Oberflächenelemente zur Pflege.
    render(): React.JSX.Element {
        return (
            <tr className='vcrnet-editexception'>
                <td>
                    <EditBoolean uvm={this.props.uvm.isActive} />
                </td>
                <td>{this.props.uvm.dayDisplay}</td>
                <td>
                    {this.props.uvm.startShift} Minute<span>{this.props.uvm.startShift === 1 ? '' : 'n'}</span>
                </td>
                <td>
                    {this.props.uvm.timeDelta} Minute<span>{this.props.uvm.timeDelta === 1 ? '' : 'n'}</span>
                </td>
            </tr>
        )
    }
}
