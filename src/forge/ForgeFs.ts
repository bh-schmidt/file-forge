import { dirname, join, relative } from "path";
import { GlobOptions, IForgeFs, WriteOptions } from "../types";
import { Forge } from "./Forge";
import fs from 'fs-extra'
import { GlobHelper } from "../common/GlobHelper";
import { PathHelper } from "../common/PathHelper";

export class ForgeFs implements IForgeFs {
    constructor(private forge: Forge) {
    }

    async ensureDirectory(path: string) {
        const newPath = this.forge.paths.targetPath(path)
        await fs.ensureDir(newPath)
    }

    async writeFile(path: string,
        data: string | NodeJS.ArrayBufferView,
        options?: WriteOptions,
    ) {
        const targetPath = this.forge.paths.targetPath(path)
        await this.ensureDirectory(dirname(targetPath))

        const fileExists = await fs.exists(targetPath)

        if (!fileExists) {
            await fs.writeFile(targetPath, data)
            return
        }

        if (options?.ifFileExists == 'replace') {
            await fs.writeFile(targetPath, data)
            return
        }

        if (options?.ifFileExists == 'ignore') {
            return
        }

        throw new Error(`The file '${targetPath}' already exists`)
    }

    async copyFile(src: string, dest: string, options: WriteOptions = {}) {
        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = this.forge.paths.targetPath(dest)

        await this.ensureDirectory(dirname(targetPath))

        const fileExists = await fs.exists(targetPath)

        if (!fileExists) {
            await fs.copy(sourcePath, targetPath)
            return
        }

        if (options?.ifFileExists == 'replace') {
            await fs.copy(sourcePath, targetPath, { overwrite: true })
            return
        }

        if (options?.ifFileExists == 'ignore') {
            return
        }

        throw new Error(`The file '${targetPath}' already exists`)
    }

    async copyDirectory(src: string, dest: string, options: WriteOptions = {}) {
        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = this.forge.paths.targetPath(dest)

        const stat = await fs.stat(sourcePath)
        if (stat.isFile()) {
            throw new Error('src must be a directory')
        }

        const directories = GlobHelper.globAll('**/*', {
            cwd: sourcePath,
            nofiles: true,
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
            const destinationPath = join(targetPath, file)
            await this.copyFile(file, destinationPath, options)
        }
    }

    async inject(pattern: string | string[], variables?: any, globOptions?: GlobOptions, writeOptions?: WriteOptions) {
        variables ??= this.forge.variables.getValues()
        const sourceDir = this.forge.paths.tempPath()

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

    async injectFile(src: string, dest: string, variables: any, writeOptions: WriteOptions = {}) {
        variables ??= this.forge.variables.getValues()
        const sourcePath = this.forge.paths.sourcePath(src)
        const targetPath = PathHelper.injectPath(this.forge.paths.targetPath(dest), variables)

        await this.ensureDirectory(dirname(targetPath))

        const fileExists = await fs.exists(targetPath)

        if (!fileExists || writeOptions?.ifFileExists == 'replace') {
            if (await this.forge._fileInjector.shouldInject(sourcePath, targetPath, variables)) {
                await this.forge._fileInjector.inject(sourcePath, targetPath, variables)
                return
            }

            await this.copyFile(sourcePath, targetPath)
            return
        }

        if (writeOptions?.ifFileExists == 'ignore') {
            return
        }

        throw new Error(`The file '${targetPath}' already exists`)
    }

    async injectDirectory(dest: string, variables: any) {
        const targetPath = PathHelper.injectPath(dest, variables)
        await this.ensureDirectory(targetPath)
    }

    async readFileSrc(path: string) {
        const sourcePath = this.forge.paths.sourcePath(path)

        if (await fs.exists(sourcePath)) {
            return await fs.readFile(sourcePath)
        }

        return undefined
    }

    async readFileTarget(path: string) {
        const targetPath = this.forge.paths.targetPath(path)

        if (await fs.exists(targetPath)) {
            return await fs.readFile(targetPath)
        }

        return undefined
    }

    async createReadStreamSrc(path: string): Promise<fs.ReadStream> {
        const sourcePath = this.forge.paths.sourcePath(path)
        return fs.createReadStream(sourcePath)
    }

    async createReadStreamTarget(path: string): Promise<fs.ReadStream> {
        const targetPath = this.forge.paths.targetPath(path)
        return fs.createReadStream(targetPath)
    }

    async existsSrc(path: string): Promise<boolean> {
        const sourcePath = this.forge.paths.sourcePath(path)
        return fs.exists(sourcePath)
    }

    async existsTarget(path: string): Promise<boolean> {
        const targetPath = this.forge.paths.targetPath(path)
        return fs.exists(targetPath)
    }
}