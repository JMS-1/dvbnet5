import * as React from 'react'
import { Component } from '../../lib.react/reactUi'
import { IGuideEntry } from '../../app/pages/guide/entry'
import { InternalLink } from '../../lib.react/command/internalLink'

// React.Js Komponente zur Anzeige einer Sendung aus der Programmzeitschrift.
export class GuideEntry extends Component<IGuideEntry> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <tr className='vcrnet-guideentry'>
                <td>{this.props.uvm.startDisplay}</td>
                <td>{this.props.uvm.endDisplay}</td>
                <td>{this.props.uvm.source}</td>
                <td>
                    <InternalLink view={() => this.props.uvm.toggleDetail()}>{this.props.uvm.name}</InternalLink>
                </td>
            </tr>
        )
    }
}
