import { globStream as asyncGlob, GlobOptions as BaseGlobOptions, Glob } from "glob";
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
            if (options.nofiles && PathHelper.isDirectory(path)) {
                continue
            }

            yield path
        }
    }
}