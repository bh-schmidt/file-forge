import fs from 'fs-extra'
import { glob } from "glob"
import os from 'os'
import { basename, dirname, isAbsolute, join } from "path"
import { GlobHelper } from "./GlobHelper"

export namespace HyperForgeData {
    export interface ClonedRepositories {
        id: string
        repo: string
        branch: string
        commit: string
        shortCommit: string
    }

    export const rebuildStrategies = ['dist-missing', 'always', 'ask'] as const;
    export type RebuildStrategy = typeof rebuildStrategies[number];

    export interface InstalledForge {
        id: string
        directory: string
        repositoryId?: string
        rebuildStrategy?: RebuildStrategy
    }

    export interface ConfigObject {
        forges: Record<string, InstalledForge>
        repositories: ClonedRepositories[]
    }

    export interface TaskInfo {
        id: string
        name: string
        filePath: string,
        default: boolean,
        description?: string
    }

    export interface ForgeInfo {
        id: string
        name: string
        isTypescript: boolean
        distExist: boolean
        directory: string
        tasks: TaskInfo[]
        description?: string
        rebuildStrategy?: RebuildStrategy
    }

    export function getDataPath(...path: string[]) {
        const p = join(...path)

        if (isAbsolute(p))
            throw 'the provided path is absolute'

        const base = process.platform == 'win32'
            ? process.env.APPDATA || join(os.homedir(), 'AppData', 'Roaming')
            : process.env.XDG_DATA_HOME || join(os.homedir(), '.local', 'share');

        return join(base, 'hyper-forge', p)
    }

    export function getGitForgesPath(...path: string[]) {
        const p = join(...path)

        if (isAbsolute(p))
            throw 'the provided path is absolute'

        return getDataPath('git', p)
    }

    export function getConfigPath() {
        return getDataPath('config.json')
    }

    export async function readConfig(): Promise<ConfigObject> {
        const path = getConfigPath()

        const defaultConfig: ConfigObject = {
            forges: {},
            repositories: []
        }

        if (!await fs.exists(path)) {
            return defaultConfig
        }

        const result = await fs.readJson(path)

        return {
            ...defaultConfig,
            ...result
        }
    }

    export async function saveConfig(config: ConfigObject) {
        const path = getConfigPath()
        await fs.writeJson(path, config, {
            spaces: 2
        })
    }

    export async function readForges() {
        const config = await readConfig()
        const entries = Object.entries(config.forges)

        const forges: ForgeInfo[] = []

        for (const [_, forgeConfig] of entries) {
            const forge = await readForgeDir(forgeConfig!.directory)
            if (forge) {
                forge.rebuildStrategy = forgeConfig.rebuildStrategy
                forges.push(forge)
            }
        }

        return forges
    }

    export async function readForgeDir(forgeDir: string): Promise<ForgeInfo | undefined> {
        if (!await fs.exists(forgeDir)) {
            return undefined
        }

        const packageJson = join(forgeDir, 'package.json')

        if (!await fs.exists(packageJson)) {
            return undefined
        }

        const json = await fs.readJSON(packageJson)
        const id = json?.name
        if (!id) {
            return undefined
        }

        const isTypescript = await GlobHelper.exists('tsconfig.json', { cwd: forgeDir })

        let search: string[]
        let distExist: boolean

        if (await fs.exists(join(forgeDir, 'dist'))) {
            search = ['dist/*/index.+(js|mjs|cjs)']
            distExist = true
        } else {
            search = ['scripts/*/index.+(js|mjs|cjs|ts)']
            distExist = false
        }

        const files = await glob(search, {
            cwd: forgeDir,
            mark: true,
            absolute: true,
            stat: true,
            nodir: true
        })

        return {
            id: id,
            name: json?.['forge']?.['name'] ?? id,
            directory: forgeDir,
            isTypescript: isTypescript,
            distExist: distExist,
            description: json?.['forge']?.['description'],
            tasks: files.map(filePath => {
                const id = basename(dirname(filePath))

                return {
                    id: id,
                    name: json?.['forge']?.[id]?.['name'] ?? id,
                    filePath: filePath,
                    description: json?.['forge']?.[id]?.['description'],
                    default: json?.['forge']?.[id]?.['default'] ?? false
                }
            })
        }
    }
}