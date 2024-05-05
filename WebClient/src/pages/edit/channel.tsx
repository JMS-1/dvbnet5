import * as React from 'react'

import { IChannelSelector } from '../../app/pages/channel'
import { SingleSelect } from '../../lib.react/edit/list'
import { ComponentWithSite } from '../../lib.react/reactUi'
import { IView } from '../../lib/site'

// Die React.Js Anzeige zur Senderauswahl.
export class EditChannel extends ComponentWithSite<IChannelSelector> implements IView {
    // Anzeigeelemente erstellen.
    render(): JSX.Element {
        return (
            <div className='vcrnet-editchannel'>
                <SingleSelect uvm={this.props.uvm.sourceName} />
                <SingleSelect uvm={this.props.uvm.section} />
                {this.props.uvm.showFilter && <SingleSelect uvm={this.props.uvm.type} />}
                {this.props.uvm.showFilter && <SingleSelect uvm={this.props.uvm.encryption} />}
            </div>
        )
    }
}
