import { clsx } from "clsx";
import HLS, { ErrorTypes } from "hls.js";
import * as React from "react";
import { IDeviceInfo } from "../../app/pages/devices/entry";
import { webCallRoot } from "../../lib/http/config";
import * as live from "../../web/IZappingContract";

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

  const [stream, setStream] = React.useState("");

  const [hls] = React.useState(new HLS());

  const video = React.useRef<HTMLVideoElement>(null);

  const { device } = props.uvm;

  React.useEffect(() => {
    live.startZapping(device);

    return () => {
      live.stopZapping(device);
    };
  }, [device]);

  React.useEffect(() => {
    setStream(
      status?.target?.startsWith("LIVE@") ? status.target.substring(5) : ""
    );

    const timer = setTimeout(
      () => live.getZappingStatus(device).then(setStatus),
      1000
    );

    return () => clearTimeout(timer);
  }, [device, status]);

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

    return () => hls.destroy();
  }, [stream, hls]);

  const select = React.useCallback(() => {
    live.setZappingSource(device, "(1,1107,17501)");
  }, [device]);

  return (
    <div className={clsx("vcrnet-live-video", props.className)}>
      <div>
        <button onClick={select}>Go</button>
        <button onClick={props.close}>Close</button>
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
  );
};
