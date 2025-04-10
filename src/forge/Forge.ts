import { Command } from "commander";
import { EventEmitter } from "../common/EventEmitter";
import { LiquidInjector, IFileInjector } from "../Injectors/LiquidInjector";
import { ForgePaths } from "./ForgePaths";
import { MemForgeFs } from "./MemForgeFs";
import { ConfigureCommand, ForgeEventAction, ForgeStages, ValidateCommands, ValidateOptions as ValidateOptions } from "../types";
import { ForgeFs } from "./ForgeFs";
import { ForgeVariables, ForgeVariablesOptions } from "./ForgeVariables";
import { ForgePrompts, ForgePromptsOptions } from "./ForgePrompts";
import { join } from "path";
import { TempDir } from "../common/TempDir";
import { ForgeRunner } from "../ForgeRunner";
import { TempFs } from "./TempFs";
import { ForgeProgram } from "./ForgeProgram";
import { ForgeConfig, ForgeConfigOptions } from "./ForgeConfig";

export interface ExecutionArgs {
    program: Command
    targetDir: string
    rootDir: string
    taskName: string
}

export class Forge {
    private runner?: ForgeRunner
    _eventEmitter: EventEmitter
    _fileInjector: IFileInjector

    taskName: string = null!
    currentStage: ForgeStages
    paths: ForgePaths
    program: ForgeProgram
    fs: ForgeFs
    memFs: MemForgeFs
    variables: ForgeVariables
    prompts: ForgePrompts
    config: ForgeConfig

    constructor() {
        this._fileInjector = new LiquidInjector()
        this._eventEmitter = new EventEmitter()

        this.currentStage = 'init'
        this.paths = new ForgePaths()
        this.program = new ForgeProgram(this)
        this.variables = new ForgeVariables(this)
        this.prompts = new ForgePrompts(this)
        this.config = new ForgeConfig(this)
        this.fs = new ForgeFs(this)
        this.memFs = new MemForgeFs(this)
    }

    configureCommands(configure: ConfigureCommand) {
        this.program._configureCommands = configure
        return this
    }

    validateCommands(validate: ValidateCommands) {
        this.program._validateCommands = validate
        return this
    }

    validateOptions(validate: ValidateOptions) {
        this.program._validateOptions = validate
        return this
    }

    fileInjector(injector: IFileInjector) {
        this._fileInjector = injector
        return this
    }

    configOptions(options: ForgeConfigOptions) {
        this.config._options = {
            ...this.variables._options,
            ...options
        }

        return this
    }

    variablesOptions(options: ForgeVariablesOptions) {
        this.variables._options = {
            ...this.variables._options,
            ...options
        }

        return this
    }

    promptsOptions(options: ForgePromptsOptions) {
        this.prompts._options = {
            ...this.prompts._options,
            ...options
        }

        return this
    }

    on(name: ForgeStages, action: ForgeEventAction) {
        this._eventEmitter.addListener(name, action)
        return this
    }

    async buildRunner(executionArgs: ExecutionArgs) {
        if (this.runner)
            return this.runner

        this.taskName = executionArgs.taskName
        this.program._command = executionArgs.program
        await this.program._configureCommands(executionArgs.program)

        this.paths._rootDir = executionArgs.rootDir
        this.paths._targetDir = executionArgs.targetDir
        this.paths._sourceDir = join(executionArgs.rootDir, 'templates', executionArgs.taskName)
        this.paths._scriptsDir = join(executionArgs.rootDir, 'scripts', executionArgs.taskName)
        this.paths._tempDir = TempDir.get("executions", crypto.randomUUID())

        this.memFs._tempFs = new TempFs(this.paths.tempPath(), this._fileInjector)

        return new ForgeRunner(this)
    }
}

export function createForge() {
    return new Forge()
}