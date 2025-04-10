import { globStream as asyncGlob, GlobOptions as BaseGlobOptions, Glob, globStream } from "glob";
import { PathHelper } from "./PathHelper";

export interface GlobOptions extends BaseGlobOptions {
    nofiles?: boolean
}

export namespace GlobHelper {
    export async function* globAll(
        pattern: string | string[],
        options: GlobOptions = {}
    ) {
        options = {
            ...options,
            mark: true,
            stat: true,
            dot: true
        }

        const stream = asyncGlob(pattern, options)

        for await (const item of stream) {
            const path = item as string
            if (options.nofiles && !PathHelper.isDirectory(path)) {
                continue
            }

            yield path
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