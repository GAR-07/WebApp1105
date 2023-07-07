export interface FileToCreate {
    userId: string | null,
    title: string | null,
    description: string | null,
    fileName: string,
    filePath: string,
    fileType: string[],
}
