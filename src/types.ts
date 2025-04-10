import { Command, Option } from "commander";
import { GlobOptions, ReadStream } from "fs-extra";
import prompts from "prompts";
import { Forge } from "./forge/Forge";
import { Options as ExecaOptions } from "execa";

export type ForgeStages = 'init' | 'prompt' | 'write' | 'conflicts' | 'commit' | 'rollback' | 'end'

export type PromiseOrValue<T> = T | Promise<T>

export type ForgeEventAction = (forge: Forge) => PromiseOrValue<void>

export type ConfigureCommand = (program: Command) => PromiseOrValue<void>

export type ValidateCommands = (program: Command) => PromiseOrValue<string | true | undefined>

export type ValidateOptions = (option: Option, value: any) => PromiseOrValue<string | true | undefined>

export type ConfigScope = 'task' | 'project'

export type FileExistsAction = 'ask' | 'ignore' | 'replace' | 'throw'

export type Writable<T> = {
    -readonly [P in keyof T]: T[P];
};

export interface WriteOptions {
    ifFileExists?: FileExistsAction
}

export type TempPathType = 'file' | 'directory';

export interface PromptObject<T extends string = string> extends prompts.PromptObject<T> { }

export interface IForgeFs {
    writeFile(path: string, data: string | NodeJS.ArrayBufferView, options?: WriteOptions): Promise<void>
    ensureDirectory(path: string): Promise<void>

    copyFile(src: string, dest: string, options: WriteOptions): Promise<void>
    copyDirectory(src: string, dest: string, options: WriteOptions): Promise<void>

    inject(pattern: string | string[], variables?: any, globOptions?: GlobOptions, writeOptions?: WriteOptions): Promise<void>
    injectFile(src: string, dest: string, variables: any, writeOptions?: WriteOptions): Promise<void>
    injectDirectory(path: string, variables: any): Promise<void>

    readFileSrc(srcPath: string): Promise<Buffer | undefined>
    readFileTarget(srcPath: string): Promise<Buffer | undefined>

    createReadStreamSrc(path: string): Promise<ReadStream>
    createReadStreamTarget(path: string): Promise<ReadStream>

    existsSrc(path: string): Promise<boolean>
    existsTarget(path: string): Promise<boolean>
}

export type RunCommandOptions<TOptions> = {
    args?: string[]
    execaOptions?: TOptions
    printResult?: boolean
    printCommand?: boolean
    printStdout?: boolean
    printStderr?: boolean
}
