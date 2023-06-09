export interface Image {
    userId: string,
    imgName: string,
    imgPath: [string, ...string[]] ,
    imgWidth: [number, ...number[]],
    imgHeight: [number, ...number[]],
    title: string | null,
    description: string | null,
}