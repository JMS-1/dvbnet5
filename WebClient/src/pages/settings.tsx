import * as React from 'react'

import { ISettingsPage } from '../app/pages/settings'
import { Field } from '../common/field'
import { HelpLink } from '../common/helpLink'
import { InlineHelp } from '../common/inlineHelp'
import { ButtonCommand } from '../lib.react/command/button'
import { InternalLink } from '../lib.react/command/internalLink'
import { EditBoolean } from '../lib.react/edit/boolean/flag'
import { SingleSelect } from '../lib.react/edit/list'
import { EditNumber } from '../lib.react/edit/number/number'
import { ComponentWithSite } from '../lib.react/reactUi'

// React.Js Komponente zur Pflege der Benutzereinstellungen.
export class Settings extends ComponentWithSite<ISettingsPage> {
    // Oberflächenelemente erzeugen.
    render(): React.JSX.Element {
        return (
            <div className='vcrnet-settings'>
                Hier werden im Wesentlichen Voreinstellungen für einzelne Teil der Web Anwendung des VCR.NET Recording
                Service festgelegt.
                <form>
                    {this.getPlanHelp()}
                    <Field label={`${this.props.uvm.planDays.text}:`} page={this.props.uvm}>
                        <EditNumber chars={5} uvm={this.props.uvm.planDays} />
                    </Field>
                    {this.getSourceHelp()}
                    <div className='vcrnet-settings-field'>
                        Inhalte der Senderlisten bei Aufzeichnungen:
                        <SingleSelect uvm={this.props.uvm.sourceType} />
                        <SingleSelect uvm={this.props.uvm.encryption} />
                    </div>
                    <Field label={`${this.props.uvm.maxFavorites.text}:`} page={this.props.uvm}>
                        <EditNumber chars={5} uvm={this.props.uvm.maxFavorites} />
                    </Field>
                    <div className='vcrnet-settings-field'>
                        Bevorzugte Zusatzoptionen für Aufzeichnungen:
                        <EditBoolean uvm={this.props.uvm.dolby} />
                        <EditBoolean uvm={this.props.uvm.allAudio} />
                        <EditBoolean uvm={this.props.uvm.ttx} />
                        <EditBoolean uvm={this.props.uvm.subs} />
                    </div>
                    {this.getGuideHelp()}
                    <Field label={`${this.props.uvm.guideRows.text}:`} page={this.props.uvm}>
                        <EditNumber chars={5} uvm={this.props.uvm.guideRows} />
                    </Field>
                    <Field label={`${this.props.uvm.preGuide.text}:`} page={this.props.uvm}>
                        <EditNumber chars={5} uvm={this.props.uvm.preGuide} />
                    </Field>
                    <Field label={`${this.props.uvm.postGuide.text}:`} page={this.props.uvm}>
                        <EditNumber chars={5} uvm={this.props.uvm.postGuide} />
                    </Field>
                    <div className='vcrnet-settings-field'>
                        <EditBoolean uvm={this.props.uvm.backToGuide} />
                    </div>
                    <div>
                        <ButtonCommand uvm={this.props.uvm.update} />
                    </div>
                </form>
            </div>
        )
    }

    // Hilfe zum Aufzeichnungsplan.
    private getPlanHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen'>
                Im Aufzeichnungsplan
                <InternalLink pict='plan' view={this.props.uvm.application.planPage.route} /> werden die Daten aller
                geplanten Aufzeichnungen in einer Liste angezeigt. Um eine gewisse Übersichtlichkeit zu erhalten wird
                allerdings nur eine begrenzte Anzahl von Aufzeichnungen auf einmal angezeigt. Die im Folgenden
                angezeigte Zahl legt fest, wie viele Tage pro Seite im Aufzeichnungsplan berücksichtigt werden sollen.
            </InlineHelp>
        )
    }

    // Hilfe zur Auswahl der Quellen.
    private getSourceHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen'>
                Bei der Programmierung neuer Aufzeichnungen können eine ganze Reihe von Einstellungen verwendet werden,
                die bei der Auswahl der zu verwendenden Quelle helfen.
                <HelpLink page={this.props.uvm} topic='sourcechooser' /> Hier wird die gewünschte Vorbelegung dieser
                Einstellungen festgelegt.
            </InlineHelp>
        )
    }

    // Hilfe zur Programmzeitschrift.
    private getGuideHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen'>
                Wird eine neue Aufzeichnung aus der Programmzeitschrift
                <HelpLink page={this.props.uvm} topic='epg' /> heraus angelegt, so können hier vor allem die Vor- und
                Nachlaufzeiten der Aufzeichnung relativ zu den exakten Daten aus der Programmzeitschrift festgelegt
                werden. Es handelt sich allerdings nur um Vorschlagswerte, die in den Daten der neuen Aufzeichnung
                jederzeit korrigiert werden können. Da Sendungen in den seltensten Fällen genau wie von den
                Sendeanstalten geplant beginnen, macht der Einsatz dieser Zeiten im Allgemeinen sehr viel Sinn - selbst
                wenn wie bei der Tagesschau zumindest der Startzeitpunkt sehr exakt festliegt kann es immer noch sein,
                dass der Rechner, auf dem der VCR.NET Recording Service ausgeführt wird, nicht mit dieser genauen Zeit
                synchronisiert ist.
                <br />
                <br />
                Hier wird auch festgelegt, wie viele Einträge die Programmzeitschrift
                <InternalLink pict='guide' view={this.props.uvm.application.guidePage.route} /> pro Seite anzeigen soll.
                Zu große Werte erhöhen nicht nur die Zeit zur Anzeige einer Seite sondern sorgen oft auch dafür, dass
                nicht alle Sendungen einer Seite auf einen Blick erfasst werden können.
                <br />
                <br />
                Wenn die Programmierung einer Aufzeichnung aus der Programmzeitschrift abgeschlossen ist wird
                normalerweise zum Aufzeichnungsplan
                <InternalLink pict='plan' view={this.props.uvm.application.planPage.route} /> gewechselt. Ist die unten
                als letztes angebotene Einstellung aktiviert wird in diesem Fall erneut die Programmzeitschrift
                aufgerufen.
            </InlineHelp>
        )
    }
}
