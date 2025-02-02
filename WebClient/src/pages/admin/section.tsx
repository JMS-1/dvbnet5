import * as React from 'react'

import { ISection } from '../../app/pages/admin/section'
import { ButtonCommand } from '../../lib.react/command/button'
import { ComponentWithSite } from '../../lib.react/reactUi'

// Hilfskomponente zum Erstellen von Ract.JS Konfigurationskomponenten.
export abstract class AdminSection<TSectionType extends ISection> extends ComponentWithSite<TSectionType> {
    // Oberflächenelemente erstellen.
    render(): React.JSX.Element {
        return (
            <div className='vcrnet-admin-section'>
                <h2>{this.title}</h2>
                {this.renderSection()}
                <div>
                    <ButtonCommand uvm={this.props.uvm.update} />
                </div>
            </div>
        )
    }

    // Die Überschrift für diesen Bereich.
    protected abstract readonly title: string

    // Oberflächenelemente erstellen.
    protected abstract renderSection(): React.JSX.Element
}
