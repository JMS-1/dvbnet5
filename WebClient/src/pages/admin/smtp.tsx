import * as React from 'react'

import { AdminSection } from './section'

import { IAdminSmtpPage, SmtpSection } from '../../app/pages/admin/smtp'
import { Field } from '../../common/field'
import { EditText } from '../../lib.react/edit/text/text'
import { IAdminSectionFactory } from '../admin'

// React.Js Komponente zur Konfiguration sonstiger Einstellungen.
export class AdminSmtp extends AdminSection<IAdminSmtpPage> {
    // Das zugehörige Ui View Model.
    static get uvm(): IAdminSectionFactory<IAdminSmtpPage> {
        return SmtpSection
    }

    // Die Überschrift für diesen Bereich.
    protected readonly title = 'Benachrichtigungen'

    // Erstellt die Oberflächenelemente.
    protected renderSection(): React.JSX.Element {
        return (
            <div className='vcrnet-admin-smtp'>
                Mit diesen Einstellungen kann der VCR.NET Recording Service dazu aufgefordert werden, im Abschluss an
                Aufzeichnungen eine E-Mail zu versenden.
                <form>
                    <Field label={`${this.props.uvm.relay.text}:`} page={this.props.uvm.page}>
                        <EditText chars={20} uvm={this.props.uvm.relay} />
                    </Field>
                    <Field label={`${this.props.uvm.username.text}:`} page={this.props.uvm.page}>
                        <EditText chars={40} uvm={this.props.uvm.username} />
                    </Field>
                    <Field label={`${this.props.uvm.password.text}:`} page={this.props.uvm.page}>
                        <EditText password chars={10} uvm={this.props.uvm.password} />
                    </Field>
                    <Field label={`${this.props.uvm.recipient.text}:`} page={this.props.uvm.page}>
                        <EditText chars={40} uvm={this.props.uvm.recipient} />
                    </Field>
                </form>
            </div>
        )
    }
}
