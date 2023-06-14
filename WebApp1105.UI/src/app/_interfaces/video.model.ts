export interface Video {
    userId: string,
    videoName: string,
    videoPath: [string, ...string[]] ,
    videoWidth: [number, ...number[]],
    videoHeight: [number, ...number[]],
    title: string | null,
    description: string | null,
}