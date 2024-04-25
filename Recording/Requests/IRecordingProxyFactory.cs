using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public interface IRecordingProxyFactory
{ /// <summary>
  /// Beschreibt einen Aufzeichnungsauftrag, der sich aus mehreren Einzelaufzeichnungen
  /// auch auf mehreren Quellen zusammensetzen kann.
  /// </summary>
  /// <param name="state">Der Zustands des zugehörigen Geräteprofils.</param>
  /// <param name="firstRecording">Die erste Aufzeichnung, auf Grund derer dieser Zugriff angelegt wurde.</param>
    RecordingProxy Create(IProfileState state, VCRRecordingInfo firstRecording);
}

