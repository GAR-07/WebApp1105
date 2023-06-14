export interface ImageToCreate {
    userId: string | null,
    title: string | null,
    description: string | null,
    filePath: string,
    fileType: string[],
}