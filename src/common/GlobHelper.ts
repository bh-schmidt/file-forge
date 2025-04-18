import { globStream, Path } from "glob";
import { GlobOptions } from "../types";

export namespace GlobHelper {
    export async function* globAll<TOptions extends GlobOptions = GlobOptions>(
        pattern: string | string[],
        options: TOptions = {} as any
    ) {
        pattern = Array.isArray(pattern) ? pattern : [pattern]
        if (options?.nofiles) {
            pattern = pattern.map(p => p.endsWith('/') ?
                p :
                p + '/')
        }

        options = {
            ...options,
            mark: true,
            stat: true,
            dot: true
        }

        const stream = globStream(pattern, options)

        for await (const item of stream) {
            yield item as TOptions extends { withFileTypes: true } ? Path : string
        }
    }

    export async function exists(pattern: string | string[], options: GlobOptions = {}) {
        const paths = globStream(pattern, options)

        for await (const _ of paths) {
            return true
        }

        return false
    }
}