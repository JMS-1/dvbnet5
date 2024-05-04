import * as React from 'react'
import { IGuideEntry } from '../../app/pages/guide/entry'
import { Field } from '../../common/field'
import { ButtonCommand } from '../../lib.react/command/button'
import { SingleSelect } from '../../lib.react/edit/list'
import { IComponent, ComponentEx } from '../../lib.react/reactUi'
import { IGuidePage } from '../../app/pages/guide'
import { GuideEntryInfo } from './info'

// Konfiguration zur Anzeige der Details zu einer Sendung der Programmzeitschrift.
interface IGuideDetails extends IComponent<IGuideEntry> {
    // Der zugehörige Navigationsbereich.
    page: IGuidePage
}

// React.Js Komponente zur Anzeige der Details einer Sendung der Programmzeitschrift.
export class GuideDetails extends ComponentEx<IGuideEntry, IGuideDetails> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <form className='vcrnet-guideentrydetails'>
                <fieldset>
                    <GuideEntryInfo uvm={this.props.uvm} />
                </fieldset>
                <div>
                    <ButtonCommand uvm={this.props.uvm.createNew} />
                    <Field page={this.props.page} label={`(${this.props.uvm.jobSelector.text}:`}>
                        <SingleSelect uvm={this.props.uvm.jobSelector} />)
                    </Field>
                </div>
            </form>
        )
    }
}
