/* Original two-owned-output adapter. Reuse our PNG adapter's public-API helpers;
 * the other main is discarded by section GC. No upstream implementation code. */
#define main lokad_unused_png_main
#include "ffmpeg-png-warm.c"
#undef main

static void pair(Work *work, AVFrame *first)
{
    apply(work);
    av_frame_move_ref(first, work->decoded);
    apply(work); /* The first frame's storage remains owned throughout. */
}
static void release_pair(Work *work, AVFrame *first)
{
    av_frame_unref(first);
    release(work);
}
static void check_pair(Work *work, AVFrame *first, const char *path)
{
    pair(work, first);
    if (first->buf[0]->buffer == work->decoded->buf[0]->buffer)
        fail("Two retained PNG results alias.");
    check_frame(first, work, 0); check_frame(work->decoded, work, 0);
    first->data[0][0] ^= 255;
    check_frame(work->decoded, work, 0);
    av_frame_unref(first);
    check_frame(work->decoded, work, 0);
    FILE *output = fopen(path, "wb"); if (!output) fail("Open result failed.");
    for (int y = 0; y < work->height; y++)
        if (fwrite(work->decoded->data[0] + (size_t)y * work->decoded->linesize[0], 1,
            (size_t)work->row_bytes, output) != (size_t)work->row_bytes) fail("Write RGBA failed.");
    if (fclose(output)) fail("Close result failed.");
    release(work); check_source(work);
}
int main(int argc, char **argv)
{
    if (argc != 4) fail("Expected resident PNG, original RGBA, output path.");
    const char *scalar = getenv("LOKAD_IMAGE_WARM_SCALAR");
    if (scalar && !strcmp(scalar, "1")) av_force_cpu_flags(0);
    av_log_set_level(AV_LOG_ERROR);
    Work work = {0}; work.width = 513; work.height = 259; work.depth = 8;
    work.row_bytes = 513 * 4; work.cpu = affinity_cpu();
    work.original = read_file(argv[2], &work.raw_bytes);
    if (work.raw_bytes != (size_t)work.row_bytes * work.height) fail("Original size differs.");
    work.encoded = read_file(argv[1], &work.encoded_bytes);
    check_profile(work.encoded, work.encoded_bytes, 513, 259, 8, 0, 6);
    work.codec = context(0, 513, 259, 8, 6, 6);
    work.decoded = av_frame_alloc(); work.converted = av_frame_alloc();
    AVFrame *first = av_frame_alloc(); work.input = av_packet_alloc();
    if (!work.decoded || !work.converted || !first || !work.input) fail("Wrapper allocation failed.");
    require(av_new_packet(work.input, (int)work.encoded_bytes), "Prepare padded resident input");
    work.input->flags |= AV_PKT_FLAG_KEY;
    memcpy(work.input->data, work.encoded, work.encoded_bytes);
    check_pair(&work, first, argv[3]); uint64_t completed = 1;
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-retained2-decode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"activeThreadType\":0,\"outputBytes\":1062936,\"initialCompletedOperations\":1,\"twoCompleteDecodesTimed\":true,\"firstRetainedThroughSecond\":true,\"independentLiveOutputs\":true,\"survivorAfterMutationAndRelease\":true,\"outputAcquisitionAndReleaseTimed\":true,\"residentInputPreparedOutsideTiming\":true,\"scalarMode\":%s,\"ffmpegVersion\":\"%s\"}\n",
        work.cpu, scalar && !strcmp(scalar,"1") ? "true" : "false", av_version_info());
    fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            check_pair(&work, first, argv[3]); completed++;
            printf("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}\n"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000)) fail("Invalid sample.");
        uint64_t cpu_start = process_cpu_ns(), start = now_ns();
        for (uint64_t i = 0; i < count; i++) { pair(&work, first); release_pair(&work, first); }
        uint64_t elapsed = now_ns() - start, cpu_elapsed = process_cpu_ns() - cpu_start; completed += count;
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"outputBytes\":1062936,\"cpu\":%d}\n",
            count, elapsed, cpu_elapsed, completed, work.observed, work.stride, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_frame_free(&first); av_frame_free(&work.decoded); av_frame_free(&work.converted);
    av_packet_free(&work.input); avcodec_free_context(&work.codec); free(work.original); free(work.encoded);
    return 0;
}
