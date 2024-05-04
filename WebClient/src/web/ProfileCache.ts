import { getProfileInfos, IProfileInfoContract } from './IProfileInfoContract'

// Verwaltet die Geräteprofile
export class ProfileCache {
    // Die zwischengespeicherten Geräte
    private static promise: Promise<IProfileInfoContract[]>

    // Ruft die Profile ab
    static getAllProfiles(): Promise<IProfileInfoContract[]> {
        // Einmalig erzeugen.
        if (!ProfileCache.promise) {
            ProfileCache.promise = new Promise<IProfileInfoContract[]>((success, failure) => {
                // Ladevorgang anstossen.
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                getProfileInfos().then((data) => success(data!))
            })
        }

        // Verwaltung melden.
        return ProfileCache.promise
    }
}
