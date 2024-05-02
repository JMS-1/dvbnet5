module VCRServer {
    // Repräsentiert die Klasse SchedulerRules
    export interface SchedulerRulesContract extends SettingsContract {
        // Die Liste der Regeln
        rules: string
    }

    export function getSchedulerRules(): Promise<SchedulerRulesContract | undefined> {
        return doUrlCall(`configuration?rules`)
    }

    export function setSchedulerRules(data: SchedulerRulesContract): Promise<boolean | undefined> {
        return doUrlCall(`configuration?rules`, `PUT`, data)
    }
}
