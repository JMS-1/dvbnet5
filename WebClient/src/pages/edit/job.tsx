import * as React from 'react'
import { IJobEditor } from '../../app/pages/edit/job'
import { Field } from '../../common/field'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { SingleSelect } from '../../lib.react/edit/list'
import { EditText } from '../../lib.react/edit/text/text'
import { EditChannel } from './channel'
import { EditChannelFlags } from './channelFlags'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Pflege der Daten eines Auftrags.
export class JobData extends Component<IJobEditor> {
    // Oberflächenelement anlegen.
    render(): JSX.Element {
        return (
            <fieldset className='vcrnet-jobdata'>
                <legend>Daten zum Auftrag</legend>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.device.text}:`}>
                    <SingleSelect uvm={this.props.uvm.device} />
                    <EditBoolean uvm={this.props.uvm.deviceLock} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.name.text}:`} help='jobsandschedules'>
                    <EditText uvm={this.props.uvm.name} chars={100} hint='(Jeder Auftrag muss einen Namen haben)' />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.folder.text}:`}>
                    <SingleSelect uvm={this.props.uvm.folder} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.source.text}:`} help='sourcechooser'>
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.sourceFlags.text}:`} help='filecontents'>
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>
            </fieldset>
        )
    }
}
