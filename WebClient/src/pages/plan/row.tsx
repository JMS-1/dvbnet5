import * as React from 'react'

import { IPlanEntry } from '../../app/pages/plan/entry'
import { InternalLink } from '../../lib.react/command/internalLink'
import { Pictogram } from '../../lib.react/command/pictogram'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Anzeige einer Aufzeichnung im Aufzeichnungsplan.
export class PlanRow extends Component<IPlanEntry> {
    // Oberflächenelemente erstellen.
    render(): React.JSX.Element {
        return (
            <tr className='vcrnet-planrow'>
                <td>{this.props.uvm.mode ? <Pictogram name={this.props.uvm.mode} /> : <span>&nbsp;</span>}</td>
                <td>
                    {this.props.uvm.mode ? (
                        <InternalLink view={() => this.props.uvm.toggleDetail(true)}>
                            {this.props.uvm.displayStart}
                        </InternalLink>
                    ) : (
                        <span>{this.props.uvm.displayStart}</span>
                    )}
                </td>
                <td className={this.props.uvm.suspectTime ? 'vcrnet-planrow-suspect' : undefined}>
                    {this.props.uvm.displayEnd}
                </td>
                <td>{this.props.uvm.station}</td>
                <td>
                    {this.props.uvm.editLink ? (
                        <InternalLink view={this.props.uvm.editLink}>{this.props.uvm.name}</InternalLink>
                    ) : (
                        <span>{this.props.uvm.name}</span>
                    )}
                </td>
                <td>
                    {this.props.uvm.exception ? (
                        <InternalLink
                            pict={this.props.uvm.exception.exceptionMode}
                            view={() => this.props.uvm.toggleDetail(false)}
                        />
                    ) : null}
                </td>
                <td>{this.props.uvm.device}</td>
            </tr>
        )
    }
}
