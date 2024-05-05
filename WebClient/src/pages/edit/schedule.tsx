import * as React from 'react'

import { EditChannel } from './channel'
import { EditChannelFlags } from './channelFlags'
import { EditDuration } from './duration'

import { IScheduleEditor } from '../../app/pages/edit/schedule'
import { Field } from '../../common/field'
import { EditBoolean } from '../../lib.react/edit/boolean/flag'
import { EditDay } from '../../lib.react/edit/datetime/day'
import { EditText } from '../../lib.react/edit/text/text'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Pflege der Daten einer Aufzeichnung.
export class ScheduleData extends Component<IScheduleEditor> {
    // Oberflächenelement anlegen.
    render(): JSX.Element {
        return (
            <fieldset className='vcrnet-scheduledata'>
                <legend>Daten zur Aufzeichnung</legend>

                <Field help='jobsandschedules' label={`${this.props.uvm.name.text}:`} page={this.props.uvm.page}>
                    <EditText chars={100} hint='(Optionaler Name der Aufzeichnung)' uvm={this.props.uvm.name} />
                </Field>

                <Field help='sourcechooser' label={`${this.props.uvm.source.text}:`} page={this.props.uvm.page}>
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field help='filecontents' label={`${this.props.uvm.sourceFlags.text}:`} page={this.props.uvm.page}>
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>

                <Field label={`${this.props.uvm.firstStart.text}:`} page={this.props.uvm.page}>
                    <EditDay uvm={this.props.uvm.firstStart} />
                    {this.props.uvm.repeat.value !== 0 && (
                        <span>
                            <span>{this.props.uvm.lastDay.text}</span>
                            <EditDay uvm={this.props.uvm.lastDay} />
                        </span>
                    )}
                </Field>

                <Field label={`${this.props.uvm.duration.text}:`} page={this.props.uvm.page}>
                    <EditDuration uvm={this.props.uvm.duration} />
                </Field>

                <Field help='repeatingschedules' label={`${this.props.uvm.repeat.text}:`} page={this.props.uvm.page}>
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
