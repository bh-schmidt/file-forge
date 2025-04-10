import fs from 'fs-extra';
import { join, sep } from "path";
import { GlobHelper, GlobOptions } from "../common/GlobHelper";
import { PathHelper } from "../common/PathHelper";
import { IForgeFs, TempPathType, WriteOptions } from "../types";
import { Forge } from "./Forge";
import { TempFs } from "./TempFs";

export class MemForgeFs implements IForgeFs {
    private disposed = false
    _tempFs: TempFs = null!

    constructor(private forge: Forge) { }

    async ensureDirectory(path: string) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        await this._tempFs.ensureDirectory(targetPath)
    }

    async writeFile(path: string,
        data: string | NodeJS.ArrayBufferView,
        options?: WriteOptions,
    ) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        await this._tempFs.writeFile(targetPath, data, options)
    }

    async copyFile(src: string, dest: string, options: WriteOptions = {}) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = this.forge.paths.targetPath(dest)

        await this._tempFs.copyFile(sourcePath, targetPath, options)
    }

    async copyDirectory(src: string, dest: string, options: WriteOptions = {}) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = this.forge.paths.targetPath(dest)

        const stat = await fs.stat(sourcePath)
        if (stat.isFile()) {
            throw new Error('src must be a directory')
        }

        const directories = GlobHelper.globAll('**/*', {
            cwd: sourcePath,
            nofiles: true
        })

        for await (const directory of directories) {
            const destinationPath = join(targetPath, directory)
            await this.ensureDirectory(destinationPath)
        }

        const files = GlobHelper.globAll('**/*', {
            cwd: sourcePath,
            nodir: true
        })

        for await (const file of files) {
            console.log('c')
            const destinationPath = join(targetPath, file)
            await this.copyFile(file, destinationPath, options)
        }
    }

    async copy(pattern: string | string[], options: WriteOptions = {}) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const sourcePath = this.forge.paths.sourcePath()

        const paths = GlobHelper.globAll(pattern, {
            cwd: sourcePath
        })

        for await (const path of paths) {
            if (PathHelper.isDirectory(path)) {
                await this.copyDirectory(path, path, options)
            } else {
                await this.copyFile(path, path, options)
            }
        }
    }

    async inject(pattern: string | string[], variables?: any, globOptions: GlobOptions = {}, writeOptions: WriteOptions = {}) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        variables ??= this.forge.variables.getValues()
        const sourceDir = this.forge.paths.sourcePath()

        const paths = GlobHelper.globAll(pattern, {
            ...globOptions,
            cwd: sourceDir,
        })

        for await (const path of paths) {
            if (PathHelper.isDirectory(path)) {
                await this.injectDirectory(path, variables)
            }
            else {
                await this.injectFile(path, path, variables, writeOptions)
            }
        }
    }

    async injectFile(src: string, dest: string, variables?: any, writeOptions: WriteOptions = {}) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        variables ??= this.forge.variables.getValues()

        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = this.forge.paths.targetPath(dest)

        await this._tempFs.injectFile(sourcePath, targetPath, variables, writeOptions)
    }

    async injectDirectory(path: string, variables?: any) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        variables ??= this.forge.variables.getValues()

        const newPath = PathHelper.injectPath(path, variables)
        await this.ensureDirectory(newPath)
    }

    async readFileSrc(path: string) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const sourcePath = this.forge.paths.sourcePath(path)
        return await this.forge.fs.readFileSrc(sourcePath)
    }

    async readFileTarget(path: string): Promise<Buffer | undefined> {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        return await this._tempFs.readFile(targetPath)
    }

    async createReadStreamSrc(path: string): Promise<fs.ReadStream> {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        return await this._tempFs.createReadStream(targetPath)
    }

    async createReadStreamTarget(path: string): Promise<fs.ReadStream> {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        return await this._tempFs.createReadStream(targetPath)
    }

    async* getDirs(sort: boolean = false) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const directories = this._tempFs.getTempPaths(
            {
                type: 'directory'
            },
            sort)
        for await (const item of directories) {
            const path = PathHelper.isDirectory(item.targetPath) ?
                item.targetPath :
                item.targetPath + sep

            yield path
        }
    }

    getTempPaths(type: TempPathType, sort: boolean = false) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        return this._tempFs.getTempPaths(
            {
                type: type
            },
            sort)
    }

    async getFileRealPath(path: string) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        const temp = await this._tempFs.getTempPath(targetPath, 'file')

        return temp?.tempPath
    }

    async existsSrc(path: string) {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        return await this.forge.fs.existsSrc(path)
    }

    async existsTarget(path: string): Promise<boolean> {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        return await this._tempFs.exists(targetPath)
    }

    async approveFile(path: string): Promise<void> {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        const targetPath = this.forge.paths.targetPath(path)
        await this._tempFs.approve(targetPath)
    }

    async commit() {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        await this._tempFs.commit()
        this.disposed = true
    }

    async rollback() {
        if (this.disposed)
            throw new Error('Memory fs is disposed')

        await this._tempFs.rollback()
        this.disposed = true
    }

    isDisposed() {
        return this.disposed
    }
}