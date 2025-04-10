import { tmpdir } from "os";
import { isAbsolute, join } from "path";

export namespace TempDir {
    export function get(...path: string[]) {
        const p = join(...path)

        if (isAbsolute(p))
            throw 'the provided path is absolute'

        return join(tmpdir(), 'hyper-forge', p)
    }
}