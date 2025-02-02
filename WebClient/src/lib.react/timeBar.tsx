import * as React from 'react'

import { Component } from './reactUi'

import { ITimeBar } from '../lib/timebar'

// Eine React.Js Komponente zur Anzeige einer Zeitschiene.
export class TimeBar extends Component<ITimeBar> {
    // Erstellt die Oberflächenelemente zur Anzeige der Zeitschiene.
    render(): React.JSX.Element {
        return (
            <div className='jmslib-timebar'>
                <div>
                    {this.props.uvm.prefixTime > 0 && <div style={{ width: `${this.props.uvm.prefixTime}%` }}></div>}
                    <div
                        className={this.props.uvm.timeIsComplete ? 'jmslib-timebar-good' : 'jmslib-timebar-bad'}
                        style={{ left: `${this.props.uvm.prefixTime}%`, width: `${this.props.uvm.time}%` }}
                    ></div>
                    {this.props.uvm.suffixTime > 0 && (
                        <div
                            style={{
                                left: `${this.props.uvm.prefixTime + this.props.uvm.time}%`,
                                width: `${this.props.uvm.suffixTime}%`,
                            }}
                        ></div>
                    )}
                </div>
                <div
                    style={
                        this.props.uvm.currentTime === undefined
                            ? { display: 'none' }
                            : { left: `${this.props.uvm.currentTime}%` }
                    }
                ></div>
            </div>
        )
    }
}
