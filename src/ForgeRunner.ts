import fs from 'fs-extra';
import { dirname, join } from "path";
import prompts from "prompts";
import { lock } from "proper-lockfile";
import { GlobHelper } from "./common/GlobHelper";
import { Forge } from "./forge/Forge";
import { FileHelper } from "./Helpers/FileHelper";
import chalk from 'chalk';
import { Command } from 'commander';

export class ForgeRunner {
    lockFilePath: string
    private releaseLock: (() => Promise<void>) | undefined

    constructor(private forge: Forge) {
        this.lockFilePath = join(forge.paths.tempPath(), '.lock')
    }

    async run() {
        await this.init();
        await this.prompt()

        try {
            await this.write()
            await this.conflicts()

            if (this.forge.currentStage !== 'rollback') {
                await this.commit();
            }
        } catch (error) {
            console.log(`An error ocurred during '${this.forge.currentStage}' stage:\n${error}\n\nRolling back...`)

            await this.rollback();
        }

        await this.end()
    }

    // async prepare()

    async init() {
        this.forge.currentStage = 'init'

        const opts = this.collectOptions(this.forge.program._command)

        const commandsResult = await this.forge.program._validateCommands(this.forge.program._command)
        if (commandsResult !== undefined && commandsResult !== true) {
            this.forge.program._command.error(`error: ${commandsResult}`)
        }

        for (const option of this.forge.program._command.options) {
            const name = option.attributeName()
            if (!(name in opts)) {
                continue
            }

            const value = opts[name];
            const result = await this.forge.program._validateOptions(option, value)
            if (result === undefined || result === true)
                continue

            this.forge.program._command.error(`error: option '${option.flags}' is invalid. ${result}`)
        }

        if (await this.forge.config.load()) {
            const newDir = dirname(this.forge.config.getConfigPath()!)
            this.forge.paths.setTargetDir(newDir)
        }

        await fs.ensureDir(this.forge.paths.tempPath())
        await fs.writeFile(this.lockFilePath, '');
        this.releaseLock = await lock(this.lockFilePath)

        await this.forge._eventEmitter.emit('init', this.forge)
    }

    async prompt() {
        this.forge.currentStage = 'prompt'
        await this.forge._eventEmitter.emit('prompt', this.forge)
    }

    async write() {
        this.forge.currentStage = 'write'
        await this.forge._eventEmitter.emit('write', this.forge)
    }

    async conflicts() {
        this.forge.currentStage = 'conflicts'

        const maps = this.forge.memFs.getTempPaths('file', true)

        for await (const map of maps) {
            if (!await this.forge.fs.existsTarget(map.targetPath)) {
                await this.forge.memFs.approveFile(map.targetPath)
                continue
            }

            if (await FileHelper.equal(map.targetPath, map.tempPath!)) {
                continue
            }

            let answer: prompts.Answers<'action'> | undefined = undefined

            if (!map.ifFileExists || map.ifFileExists == 'ask') {
                answer = await prompts({
                    name: 'action',
                    type: 'select',
                    message: `The file '${map.targetPath}' already exists\nSelect your action:`,
                    choices: [
                        {
                            title: 'Replace',
                            value: 'replace'
                        },
                        {
                            title: 'Ignore',
                            value: 'ignore'
                        },
                        {
                            title: 'Stop Execution',
                            value: 'stop'
                        },
                        {
                            title: 'Rollback',
                            value: 'rolback'
                        }
                    ],
                    initial: 0
                })
            }

            if (map.ifFileExists == 'replace' || answer?.action == 'replace') {
                await this.forge.memFs.approveFile(map.targetPath)
                continue
            }

            if (map.ifFileExists == 'ignore' || answer?.action == 'ignore') {
                continue
            }

            if (map.ifFileExists == 'throw') {
                throw new Error(`The file '${map.targetPath}' already exists.`)
            }

            if (answer?.action == 'stop') {
                console.log('Execution stopped')
                process.exit()
            }

            if (answer?.action == 'rollback') {
                await this.rollback()
                return
            }

            throw new Error('Conflict resolution not implemented')
        }

        await this.forge._eventEmitter.emit('conflicts', this.forge)
    }

    async commit() {
        this.forge.currentStage = 'commit'

        if (this.forge.config._options.autoSave && this.forge.config.hasPendingChanges()) {
            await this.forge.config.save()
        }

        await this.forge.memFs.commit()
        await this.forge._eventEmitter.emit('commit', this.forge)
    }

    async rollback() {
        this.forge.currentStage = 'rollback'

        if (this.forge.memFs.isDisposed()) {
            console.log(chalk.yellow.bold(`Changes were already committed, could not rollback. Please do it manually or configure it on rollback stage.`))
        } else {
            await this.forge.memFs.rollback()
        }

        await this.forge._eventEmitter.emit('rollback', this.forge)
    }

    async end() {
        this.forge.currentStage = 'end'
        await this.forge._eventEmitter.emit('end', this.forge)

        const paths = GlobHelper.globAll('**/*', {
            cwd: this.forge.paths.tempPath(),
            ignore: ['.lock'],
            nodir: true,
            absolute: true
        })

        for await (const path of paths) {
            await fs.rm(path)
        }

        if (this.releaseLock) {
            await this.releaseLock()
        }

        await fs.rm(this.forge.paths.tempPath(), { recursive: true })
    }

    private collectOptions(command: Command) {
        let options: any = {}
        let current: Command | null = command

        while (current) {
            options = {
                ...current.opts(),
                ...options,
            }
            current = current.parent
        }

        return options
    }
}