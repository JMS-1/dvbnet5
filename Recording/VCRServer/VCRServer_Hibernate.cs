namespace JMS.DVB.NET.Recording
{
    partial class VCRServer
    {
        /// <summary>
        /// Wird beim Aufwecken aus dem Schlafzustand aktiviert.
        /// </summary>
        public void Resume()
        {
            // Forward
            Profiles.Resume();

            // Forward to extension manager
            ExtensionProcessManager.Resume();
        }

        /// <summary>
        /// Teilt dieser VCR.NET Instanz mit, dass ein �bergang in den Schlafzugrang
        /// erfolgt ist.
        /// </summary>
        public void Suspend()
        {
            // Forward
            Profiles.Suspend();

            // Make sure that we forget anything on recording after wake up
            VCRConfiguration.Current.HasRecordedSomething = false;

            // Get the final plan respecting the fact that we are now hibernating
            Profiles.EnsureNewPlan();
        }

        /// <summary>
        /// Teilt dieser VCR.NET Instanz mit, dass ein �bergang in den Schlafzugrang
        /// erfolgen wird.
        /// </summary>
        public void PrepareSuspend()
        {
            // Forward to extension manager
            ExtensionProcessManager.Suspend();

            // Forward to hardware
            Profiles.PrepareSuspend();
        }

        /// <summary>
        /// Teilt der Laufzeitumgebung mit, dass ein Ger�teprofil eine Aufzeichnung abgeschlossen hat.
        /// </summary>
        /// <param name="disallowHibernate">Gesetzt, wenn diese Aufzeichnung keinen �bergang in
        /// den Schlafzustand ausl�sen soll.</param>
        /// <param name="realRecording">Gesetzt, wenn es sich um eine echte Aufzeichnung handelt.</param>
        internal void ReportRecordingDone(bool disallowHibernate, bool realRecording)
        {
            // Flag in volatile part of configuration
            if (realRecording)
                VCRConfiguration.Current.HasRecordedSomething = true;
        }

    }
}
