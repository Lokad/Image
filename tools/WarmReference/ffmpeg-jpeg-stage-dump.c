/* Original development-only instrumentation of our existing public-API worker.
 * The encoder and its options remain unchanged. Save the actual frame submitted
 * to avcodec_send_frame once; require the resulting packet to match its anchor.
 * No upstream implementation or internal header is a build input. */
#define _GNU_SOURCE
#include <libavcodec/avcodec.h>
#include <libavutil/pixfmt.h>
#include <stdio.h>
#include <stdlib.h>

static int capture_frame(AVCodecContext *context, const AVFrame *frame);
#define avcodec_send_frame capture_frame
#include "ffmpeg-jpeg-warm.c"
#undef avcodec_send_frame

static int capture_frame(AVCodecContext *context, const AVFrame *value)
{
    static int captured;
    if (!captured) {
        if (value->format != AV_PIX_FMT_YUVJ444P) fail("Stage probe requires full-resolution JPEG YUV.");
        const char *path = getenv("LOKAD_JPEG_STAGE_DUMP");
        if (!path || !*path) fail("Stage dump path required.");
        FILE *output = fopen(path, "wb");
        if (!output) fail("Open stage dump failed.");
        for (int component = 0; component < 3; component++)
            for (int y = 0; y < value->height; y++)
                if (fwrite(value->data[component] + (size_t)y * value->linesize[component],
                           1, (size_t)value->width, output) != (size_t)value->width)
                    fail("Stage dump write failed.");
        if (fclose(output)) fail("Stage dump close failed.");
        captured = 1;
    }
    return avcodec_send_frame(context, value);
}
