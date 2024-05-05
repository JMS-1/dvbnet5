import * as React from 'react'

import { ISourceFlagsEditor } from '../../app/pages/edit/jobSchedule'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur visuellen Darstellung der Aufzeichnungsoptionen.
export class EditChannelFlags extends Component<ISourceFlagsEditor> {
    // Erzeugt die visuelle Darstellung.
    render(): JSX.Element {
        return (
            <div className='vcrnet-editchannelflags'>
                <EditBoolean uvm={this.props.uvm.includeDolby} />
                <EditBoolean uvm={this.props.uvm.allLanguages} />
                <EditBoolean uvm={this.props.uvm.withVideotext} />
                <EditBoolean uvm={this.props.uvm.withSubtitles} />
            </div>
        )
    }
}
