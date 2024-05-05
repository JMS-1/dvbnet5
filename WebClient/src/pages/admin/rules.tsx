import * as React from 'react'

import { AdminSection } from './section'

import { IAdminRulesPage, RulesSection } from '../../app/pages/admin/rules'
import { HelpLink } from '../../common/helpLink'
import { InlineHelp } from '../../common/inlineHelp'
import { EditTextArea } from '../../lib.react/edit/text/textarea'
import { IAdminSectionFactory } from '../admin'

// React.Js Komponente zur Konfiguration der Planungsregeln.
export class AdminRules extends AdminSection<IAdminRulesPage> {
    // Das zugehörige Ui View Model.
    static get uvm(): IAdminSectionFactory<IAdminRulesPage> {
        return RulesSection
    }

    // Die Überschrift für diesen Bereich.
    protected readonly title = 'Regeln für die Planung von Aufzeichnungen'

    // Oberflächenelement anlegen.
    protected renderSection(): JSX.Element {
        return (
            <div className='vcrnet-admin-rules'>
                Der VCR.NET Recording Service verwendet nach der Installation ein festes Regelwerk zur Planung von
                Aufzeichnungen für den Fall, dass mehrere DVB.NET Geräteprofile verwendet werden. Hier ist es möglich,
                dieses Regelwerk
                <HelpLink page={this.props.uvm.page} topic='customschedule' /> anzupassen - auf eigene Gefahr, versteht
                sich.
                <EditTextArea columns={100} rows={25} uvm={this.props.uvm.rules} />
                {this.getHelp()}
            </div>
        )
    }

    // Hilfe zu den Planungsregeln.
    private getHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Für eine Aktualisierung des Regelwerks muss die entsprechende Schaltfläche explizit betätigt werden.
                Dabei wird grundsätzlich ein Neustart des VCR.NET Dienstes durchgeführt, selbst wenn keine Veränderungen
                vorgenommen wurden.
                <br />
                <br />
                Um wieder mit dem fest eingebauten Regelwerk wie nach der Erstinstallation zu arbeiten muss einfach nur
                die Eingabe geleert und eine Aktualisierung ausgelöst werden.
            </InlineHelp>
        )
    }
}
