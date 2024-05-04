import * as React from 'react'
import { Component } from '../../lib.react/reactUi'
import { IScheduleEditor } from '../../app/pages/edit/schedule'
import { Field } from '../../common/field'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { EditDay } from '../../lib.react/edit/datetime/day'
import { EditText } from '../../lib.react/edit/text/text'
import { EditChannel } from './channel'
import { EditChannelFlags } from './channelFlags'
import { EditDuration } from './duration'

// React.Js Komponente zur Pflege der Daten einer Aufzeichnung.
export class ScheduleData extends Component<IScheduleEditor> {
    // Oberflächenelement anlegen.
    render(): JSX.Element {
        return (
            <fieldset className='vcrnet-scheduledata'>
                <legend>Daten zur Aufzeichnung</legend>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.name.text}:`} help='jobsandschedules'>
                    <EditText uvm={this.props.uvm.name} chars={100} hint='(Optionaler Name der Aufzeichnung)' />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.source.text}:`} help='sourcechooser'>
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.sourceFlags.text}:`} help='filecontents'>
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.firstStart.text}:`}>
                    <EditDay uvm={this.props.uvm.firstStart} />
                    {this.props.uvm.repeat.value !== 0 && (
                        <span>
                            <span>{this.props.uvm.lastDay.text}</span>
                            <EditDay uvm={this.props.uvm.lastDay} />
                        </span>
                    )}
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.duration.text}:`}>
                    <EditDuration uvm={this.props.uvm.duration} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.repeat.text}:`} help='repeatingschedules'>
                    <EditBoolean uvm={this.props.uvm.onMonday} />
                    <EditBoolean uvm={this.props.uvm.onTuesday} />
                    <EditBoolean uvm={this.props.uvm.onWednesday} />
                    <EditBoolean uvm={this.props.uvm.onThursday} />
                    <EditBoolean uvm={this.props.uvm.onFriday} />
                    <EditBoolean uvm={this.props.uvm.onSaturday} />
                    <EditBoolean uvm={this.props.uvm.onSunday} />
                </Field>
            </fieldset>
        )
    }
}
