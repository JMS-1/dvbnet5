import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse SchedulerRules
export interface ISchedulerRulesContract extends ISettingsContract {
    // Die Liste der Regeln
    rules: string
}

export function getSchedulerRules(): Promise<ISchedulerRulesContract | undefined> {
    return doUrlCall('configuration?rules')
}

export function setSchedulerRules(data: ISchedulerRulesContract): Promise<boolean | undefined> {
    return doUrlCall('configuration?rules', 'PUT', data)
}
