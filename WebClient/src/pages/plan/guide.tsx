import * as React from 'react'

import { IPlanPage } from '../../app/pages/plan'
import { IPlanEntry } from '../../app/pages/plan/entry'
import { HelpLink } from '../../common/helpLink'
import { ComponentExWithSite, IComponent } from '../../lib.react/reactUi'
import { TimeBar } from '../../lib.react/timeBar'
import { GuideEntryInfo } from '../guide/info'

// Konfiguration zur Anzeige der Details einer Aufzeichnung.
interface IPlanGuide extends IComponent<IPlanEntry> {
    // Der zugehörige Navigationsbereich.
    page: IPlanPage
}

// React.Js Komponente zur Anzeige der Details einer Aufzeichnung.
export class PlanGuide extends ComponentExWithSite<IPlanEntry, IPlanGuide> {
    // Oberflächenelemente erstellen.
    render(): React.JSX.Element {
        const guide = this.props.uvm.guideItem

        return (
            <div className='vcrnet-planguide'>
                <div>
                    <span>Aufzeichnungsoptionen:</span>
                    <HelpLink page={this.props.page} topic='filecontents' />
                    <span className={this.props.uvm.allAudio ? undefined : 'vcrnet-optionoff'}>
                        Alle Sprachen
                    </span>,{' '}
                    <span className={this.props.uvm.dolby ? undefined : 'vcrnet-optionoff'}>Dolby Digital</span>,{' '}
                    <span className={this.props.uvm.ttx ? undefined : 'vcrnet-optionoff'}>Videotext</span>,{' '}
                    <span className={this.props.uvm.subs ? undefined : 'vcrnet-optionoff'}>DVB-Untertitel</span>,{' '}
                    <span className={this.props.uvm.guide ? undefined : 'vcrnet-optionoff'}>Programminformationen</span>
                </div>
                {guide ? (
                    <fieldset>
                        <legend>Auszug aus der Programmzeitschrift</legend>
                        <TimeBar uvm={this.props.uvm.guideTime} />
                        <GuideEntryInfo uvm={guide} />
                    </fieldset>
                ) : (
                    <div>
                        <span>Programmzeitschrift:</span> <span className='vcrnet-optionoff'>wird abgerufen</span>
                    </div>
                )}
            </div>
        )
    }
}
