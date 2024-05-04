import * as React from 'react'
import { Component } from '../../lib.react/reactUi'
import { IGuidePageNavigation } from '../../app/pages/guide'
import { ButtonCommand } from '../../lib.react/command/button'

// React.Ks Komponente zur Navigation durch die Programmzeitschrift.
export class GuideNavigation extends Component<IGuidePageNavigation> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <div className='vcrnet-epgnavigation'>
                <ButtonCommand uvm={this.props.uvm.firstPage} />
                <ButtonCommand uvm={this.props.uvm.prevPage} />
                <ButtonCommand uvm={this.props.uvm.nextPage} />
            </div>
        )
    }
}
