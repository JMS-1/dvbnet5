import * as React from 'react'

import { GuideEntryInfo } from './info'

import { IGuidePage } from '../../app/pages/guide'
import { IGuideEntry } from '../../app/pages/guide/entry'
import { Field } from '../../common/field'
import { ButtonCommand } from '../../lib.react/command/button'
import { SingleSelect } from '../../lib.react/edit/list'
import { ComponentEx, IComponent } from '../../lib.react/reactUi'

// Konfiguration zur Anzeige der Details zu einer Sendung der Programmzeitschrift.
interface IGuideDetails extends IComponent<IGuideEntry> {
    // Der zugehörige Navigationsbereich.
    page: IGuidePage
}

// React.Js Komponente zur Anzeige der Details einer Sendung der Programmzeitschrift.
export class GuideDetails extends ComponentEx<IGuideEntry, IGuideDetails> {
    // Oberflächenelemente anlegen.
    render(): React.JSX.Element {
        return (
            <form className='vcrnet-guideentrydetails'>
                <fieldset>
                    <GuideEntryInfo uvm={this.props.uvm} />
                </fieldset>
                <div>
                    <ButtonCommand uvm={this.props.uvm.createNew} />
                    <Field label={`(${this.props.uvm.jobSelector.text}:`} page={this.props.page}>
                        <SingleSelect uvm={this.props.uvm.jobSelector} />)
                    </Field>
                </div>
            </form>
        )
    }
}
