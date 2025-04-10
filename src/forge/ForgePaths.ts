import { isAbsolute, join, resolve } from "path";
import { TempDir } from "../common/TempDir";
import { ExecutionArgs } from "./Forge";

export class ForgePaths {
    _rootDir: string = null!
    _sourceDir: string = null!
    _targetDir: string = null!
    _scriptsDir: string = null!
    _tempDir: string = null!

    setRootDir(path: string) {
        if (isAbsolute(path))
            this._rootDir = path

        this._rootDir = resolve(path)
    }

    setTargetDir(path: string) {
        if (isAbsolute(path))
            this._targetDir = path

        this._targetDir = resolve(path)
    }

    setSourceDir(path: string) {
        if (isAbsolute(path))
            this._sourceDir = path

        this._sourceDir = resolve(path)
    }

    setScriptsDir(path: string) {
        if (isAbsolute(path))
            this._scriptsDir = path

        this._scriptsDir = resolve(path)
    }

    rootPath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(this._rootDir, path)
    }

    targetPath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(this._targetDir, path)
    }

    sourcePath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(this._sourceDir, path)
    }

    scriptsPath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(this._scriptsDir, path)
    }

    tempPath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(this._tempDir, path)
    }

    cwdPath(...destination: string[]) {
        const path = join(...destination)

        if (isAbsolute(path)) {
            return path
        }

        return join(process.cwd(), path)
    }
}