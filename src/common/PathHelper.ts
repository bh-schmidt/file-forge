export namespace PathHelper {
    export function isDirectory(path: string) {
        return path.endsWith('\\') || path.endsWith('/')
    }

    export function injectPath(path: string, variables: any) {
        return path.replaceAll(/\$\{([^\]]+)\}/g, (match, group) => {
            if (group in variables) {
                return variables[group] ?? ''
            }

            return match
        })
    }
}