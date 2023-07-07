export interface Book {
    id: number,
    userId: string,
    genre: string,
    author: string,
    title: string,
    annotation: string | null,
    bookFilePath: string,
    writingDate: number,
    publicationDate: number | null,
    coverPath: string,
    numberOfPages: number | null,
    publisher: string | null,
}