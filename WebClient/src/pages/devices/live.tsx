import { clsx } from "clsx";
import HLS, { ErrorTypes } from "hls.js";
import * as React from "react";
import { IDeviceInfo } from "../../app/pages/devices/entry";
import { webCallRoot } from "../../lib/http/config";
import * as live from "../../web/IZappingContract";
import { EditChannel } from "../edit/channel";

interface ILiveVideoProps {
  className?: string;
  uvm: IDeviceInfo;
}

interface ILiveVideoActions {
  close(): void;
}

export const LiveVideo = (props: ILiveVideoProps & ILiveVideoActions) => {
  const [status, setStatus] = React.useState<live.IZappingStatus | undefined>(
    undefined
  );

  const [enableStatus, setEnableStatus] = React.useState(false);

  const [stream, setStream] = React.useState("");

  const [hls] = React.useState(new HLS());

  const video = React.useRef<HTMLVideoElement>(null);

  const { device, loadSources, liveSource, getSelectedId } = props.uvm;
  const { value } = liveSource;

  React.useEffect(() => {
    loadSources()
      .then(() => live.startZapping(device))
      .then(() => setEnableStatus(true));

    return () => {
      live.stopZapping(device);

      liveSource.value = "";
    };
  }, [device, liveSource, loadSources]);

  React.useEffect(() => {
    if (!enableStatus) return;

    setStream(
      status?.target?.startsWith("LIVE@") ? status.target.substring(5) : ""
    );

    const timer = setTimeout(
      () => live.getZappingStatus(device).then(setStatus),
      1000
    );

    return () => clearTimeout(timer);
  }, [device, status, enableStatus]);

  React.useEffect(() => {
    if (!stream || !video.current) return;

    if (!HLS.isSupported()) return;

    const uri = `${webCallRoot}zapping/live/${stream}/live.m3u8`;

    hls.on(
      HLS.Events.ERROR,
      (_, data) =>
        data.type === ErrorTypes.NETWORK_ERROR &&
        setTimeout(() => hls.loadSource(uri), 500)
    );

    hls.loadSource(uri);
    hls.attachMedia(video.current);

    return () => hls.detachMedia();
  }, [stream, hls]);

  React.useEffect(() => {
    const selected = getSelectedId();

    if (!selected) return;

    live.setZappingSource(device, selected);
  }, [device, getSelectedId, value]);

  return (
    <div className={clsx("vcrnet-live-video", props.className)}>
      <div>
        <EditChannel uvm={liveSource} />
        <button onClick={props.close}>Close</button>
        <div className="video">
          {stream && [
            <video
              controls
              autoPlay
              ref={video}
              key={stream}
              style={{ display: "block" }}
            />,
          ]}
        </div>
      </div>
    </div>
  );
};
