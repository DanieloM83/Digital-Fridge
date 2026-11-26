// Scoped camera interop module for WorkspaceAddProduct
let activeStream = null;
let currentStartPromise = null;

export async function startCamera(videoElement) {
    if (!videoElement) return { success: false, error: "VIDEO_NOT_FOUND" };

    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        return { success: false, error: "CAMERA_NOT_SUPPORTED" };
    }

    // If already starting or streaming to this element, reuse the in-flight start
    if (currentStartPromise) {
        return await currentStartPromise;
    }

    currentStartPromise = (async () => {
        try {
            // If we already have an active stream running on this video, check if it's alive
            if (activeStream && videoElement.srcObject === activeStream && !videoElement.paused) {
                return { success: true };
            }

            stopCamera(videoElement);

            const constraints = {
                video: {
                    facingMode: { ideal: "environment" },
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                },
                audio: false,
            };

            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            activeStream = stream;
            videoElement.srcObject = stream;

            // Wait until stream metadata is loaded before attempting to play
            if (videoElement.readyState < 1) {
                await new Promise((resolve) => {
                    videoElement.onloadedmetadata = () => resolve();
                });
            }

            try {
                await videoElement.play();
            } catch (playErr) {
                // If play() was aborted by an unavoidable user interaction or load interruption,
                // do not treat as fatal as long as stream is active
                if (playErr.name !== "AbortError") {
                    throw playErr;
                }
            }

            return { success: true };
        } catch (err) {
            console.error("Camera access error:", err);
            return { success: false, error: err.message || "PERMISSION_DENIED" };
        } finally {
            currentStartPromise = null;
        }
    })();

    return await currentStartPromise;
}

export function stopCamera(videoElement) {
    if (activeStream) {
        activeStream.getTracks().forEach((track) => track.stop());
        activeStream = null;
    }
    if (videoElement) {
        videoElement.srcObject = null;
    }
    currentStartPromise = null;
}

export function capturePhoto(videoElement) {
    if (!videoElement || !videoElement.videoWidth) {
        return null;
    }

    // Scale to max 1024px to maintain clarity for CV/OCR while keeping payload compact
    const maxDim = 1024;
    let w = videoElement.videoWidth;
    let h = videoElement.videoHeight;
    if (w > maxDim || h > maxDim) {
        if (w > h) {
            h = Math.round((h * maxDim) / w);
            w = maxDim;
        } else {
            w = Math.round((w * maxDim) / h);
            h = maxDim;
        }
    }

    const canvas = document.createElement("canvas");
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext("2d");
    ctx.drawImage(videoElement, 0, 0, w, h);

    return canvas.toDataURL("image/jpeg", 0.75);
}
