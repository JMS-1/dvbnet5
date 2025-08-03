import { doUrlCall } from "./VCRServer";

export interface IZappingStatus {
  source?: string;
  target?: string;
}

export function startZapping(device: string): Promise<void> {
  return doUrlCall(
    `zapping/live/${encodeURIComponent(device)}?target=LIVE`,
    "POST"
  );
}

export function stopZapping(device: string): Promise<void> {
  return doUrlCall(`zapping/live/${encodeURIComponent(device)}`, "DELETE");
}

export function getZappingStatus(
  device: string
): Promise<IZappingStatus | undefined> {
  return doUrlCall(`zapping/status/${encodeURIComponent(device)}`);
}

export function setZappingSource(
  device: string,
  source: string
): Promise<IZappingStatus | undefined> {
  return doUrlCall(
    `zapping/tune/${encodeURIComponent(device)}?source=${encodeURIComponent(source)}`,
    "PUT"
  );
}
