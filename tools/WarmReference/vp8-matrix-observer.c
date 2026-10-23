/* Original development-only observer. Private native headers remain in the
 * ignored reference checkout; this is never a production build input. The
 * four source-derived susceptibilities are arguments, not native image data. */
#include <stdio.h>
#include <stdlib.h>
#include "src/enc/vp8i_enc.h"
static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static int number(const char *text,int low,int high) {
    char *end; long value=strtol(text,&end,10);
    if (!*text || *end || value<low || value>high) fail("Invalid observation input.");
    return (int)value;
}
static void matrix(const VP8Matrix *m) {
    printf("{\"q\":["); for (int p=0;p<16;p++) printf("%s%u",p?",":"",m->q[p]);
    printf("],\"iq\":["); for (int p=0;p<16;p++) printf("%s%u",p?",":"",m->iq[p]);
    printf("],\"bias\":["); for (int p=0;p<16;p++) printf("%s%u",p?",":"",m->bias[p]);
    printf("],\"threshold\":["); for (int p=0;p<16;p++) printf("%s%u",p?",":"",m->zthresh[p]);
    printf("],\"sharpen\":["); for (int p=0;p<16;p++) printf("%s%u",p?",":"",m->sharpen[p]);
    printf("]}");
}
int main(int argc,char **argv) {
    if (argc!=10) fail("Usage: observer UV_RAW ALPHA0 BETA0 ALPHA1 BETA1 ALPHA2 BETA2 ALPHA3 BETA3");
    WebPConfig config; if (!WebPConfigInit(&config)) fail("Configuration init failed.");
    config.quality=75; config.method=4; config.thread_level=0;
    VP8Encoder *enc=calloc(1,sizeof(*enc)); if (!enc) fail("Allocation failed.");
    enc->config=&config; enc->method=4; enc->segment_hdr.num_segments=4;
    enc->uv_alpha=number(argv[1],0,7905);
    for (int p=0;p<4;p++) { enc->dqm[p].alpha=number(argv[2+2*p],-127,127); enc->dqm[p].beta=number(argv[3+2*p],0,255); }
    /* Zero macroblocks avoids allocating a map. This observes parameter setup
     * given qualified susceptibilities, not native analysis or a full encode. */
    VP8SetSegmentParams(enc,75);
    printf("{\"segments\":%d,\"baseQuantizer\":%d,\"deltas\":[%d,%d,%d,%d,%d],\"parameters\":[",enc->segment_hdr.num_segments,enc->base_quant,enc->dq_y1_dc,enc->dq_y2_dc,enc->dq_y2_ac,enc->dq_uv_dc,enc->dq_uv_ac);
    for (int p=0;p<enc->segment_hdr.num_segments;p++) {
        const VP8SegmentInfo *s=&enc->dqm[p];
        printf("%s{\"quantizer\":%d,\"initialFilter\":%d,\"alpha\":%d,\"beta\":%d,\"y1\":",p?",":"",s->quant,s->fstrength,s->alpha,s->beta); matrix(&s->y1);
        printf(",\"y2\":"); matrix(&s->y2); printf(",\"uv\":"); matrix(&s->uv);
        printf(",\"lambdas\":[%d,%d,%d,%d,%d,%d,%d,%d],\"minDistortion\":%d,\"intra4Penalty\":%lld}",s->lambda_i4,s->lambda_i16,s->lambda_uv,s->lambda_mode,s->lambda_trellis_i4,s->lambda_trellis_i16,s->lambda_trellis_uv,s->tlambda,s->min_disto,(long long)s->i4_penalty);
    }
    printf("]}\n"); free(enc); return 0;
}
