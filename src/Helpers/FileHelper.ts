import { promises } from 'fs';
import fs from 'fs-extra';
import { FileHandle } from 'fs/promises';

export namespace FileHelper {
    export async function equal(path1: string, path2: string) {
        const [st1, st2] = await Promise.all([fs.stat(path1), fs.stat(path2)])

        if (st1.size !== st2.size) {
            return false;
        }

        const chunkSize = 1024 * 64

        const buf1 = Buffer.alloc(chunkSize);
        const buf2 = Buffer.alloc(chunkSize);

        let pos = 0
        let remainingSize = st1.size

        let h1, h2: FileHandle = null!

        try {
            [h1, h2] = await Promise.all([promises.open(path1), promises.open(path1)])

            while (remainingSize > 0) {
                const readSize = Math.min(chunkSize, remainingSize);
                const [res1, res2] = await Promise.all([h1.read(buf1, 0, readSize, pos), h2.read(buf2, 0, readSize, pos)])

                if (res1.bytesRead !== readSize || res2.bytesRead !== readSize) {
                    throw new Error("Failed to read desired number of bytes");
                }

                if (buf1.compare(buf2, 0, readSize, 0, readSize) !== 0) {
                    return false;
                }

                remainingSize -= readSize;
                pos += readSize;
            }

            return true
        } finally {
            if (h1) {
                await h1.close()
            }

            if (h2) {
                await h2.close()
            }
        }
    }
}