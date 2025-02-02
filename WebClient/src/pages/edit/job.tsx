import * as React from 'react'

import { EditChannel } from './channel'
import { EditChannelFlags } from './channelFlags'

import { IJobEditor } from '../../app/pages/edit/job'
import { Field } from '../../common/field'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { SingleSelect } from '../../lib.react/edit/list'
import { EditText } from '../../lib.react/edit/text/text'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Pflege der Daten eines Auftrags.
export class JobData extends Component<IJobEditor> {
    // Oberflächenelement anlegen.
    render(): React.JSX.Element {
        return (
            <fieldset className='vcrnet-jobdata'>
                <legend>Daten zum Auftrag</legend>

                <Field label={`${this.props.uvm.device.text}:`} page={this.props.uvm.page}>
                    <SingleSelect uvm={this.props.uvm.device} />
                    <EditBoolean uvm={this.props.uvm.deviceLock} />
                </Field>

                <Field help='jobsandschedules' label={`${this.props.uvm.name.text}:`} page={this.props.uvm.page}>
                    <EditText chars={100} hint='(Jeder Auftrag muss einen Namen haben)' uvm={this.props.uvm.name} />
                </Field>

                <Field label={`${this.props.uvm.folder.text}:`} page={this.props.uvm.page}>
                    <SingleSelect uvm={this.props.uvm.folder} />
                </Field>

                <Field help='sourcechooser' label={`${this.props.uvm.source.text}:`} page={this.props.uvm.page}>
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field help='filecontents' label={`${this.props.uvm.sourceFlags.text}:`} page={this.props.uvm.page}>
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>
            </fieldset>
        )
    }
}
