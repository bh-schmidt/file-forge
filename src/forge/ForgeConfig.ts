import fs from 'fs-extra'
import { dirname, join } from 'path'
import { Forge } from './Forge'
import { ConfigScope } from '../types'

const ConfigFileName = 'config.hf.json'

/**
 * Options for Config behavior.
 */
export interface ForgeConfigOptions {
    /** Whether the config should be automatically saved after changes. */
    autoSave?: boolean
}

/**
 * Shape of the configuration object stored in file and memory.
 */
interface ConfigObject {
    /** Project configuration values shared across all tasks. */
    project?: Record<string, any>
    /** Task-specific configuration values, keyed by task name. */
    tasks?: Record<string, Record<string, any>>
}

/**
 * Handles loading, saving, and manipulating project and task-specific configurations.
 */
export class ForgeConfig {
    private values?: ConfigObject = {
        project: {},
        tasks: {}
    }

    private pendingChanges: boolean = false

    private configPath?: string

    /** Config behavior options. */
    _options: ForgeConfigOptions = {
        autoSave: true,
    }

    /**
     * Create a new ForgeConfig instance.
     * @param forge The Forge instance providing context to the config object.
     */
    constructor(private forge: Forge) { }

    /**
     * Retrieve a task-specific configuration value for the current task.
     * @param key Configuration key.
     * @param scope The configuration scope to retrieve the key from. 
     * Can be `'task'` for task-specific settings or `'project'` for project-wide settings.
     * @returns A deep clone of the stored value, or undefined if not found.
     */
    get(key: string, scope: ConfigScope = 'task') {
        if (scope == 'task')
            return structuredClone(this.values?.tasks?.[this.forge.taskName]?.[key])

        return structuredClone(this.values?.project?.[key])
    }

    /**
     * Get all configuration values for the current task.
     * @param scope The configuration scope to retrieve the key from. 
     * Can be `'task'` for task-specific settings or `'project'` for project-wide settings.
     * @returns A deep clone of the task's config object, or undefined if not set.
     */
    getValues(scope: ConfigScope = 'task') {
        if (scope == 'task')
            return structuredClone(this.values?.tasks?.[this.forge.taskName])

        return structuredClone(this.values?.project)
    }

    /**
     * Sets a task-specific configuration value for the current task.
     * @param key The key to set.
     * @param scope The configuration scope to retrieve the key from. 
     * Can be `'task'` for task-specific settings or `'project'` for project-wide settings.
     * @param value The value to associate with the key.
     */
    set(key: string, value: any, scope: ConfigScope = 'task') {
        if (scope == 'task') {
            this.pendingChanges = true
            this.values ??= {}
            this.values.tasks ??= {}
            this.values.tasks[this.forge.taskName] ??= {}
            this.values.tasks[this.forge.taskName][key] = structuredClone(value)
            return
        }

        this.pendingChanges = true
        this.values ??= {}
        this.values.project ??= {}
        this.values.project[key] = structuredClone(value)
    }

    /**
     * Sets multiple task-specific configuration values for the current task.
     * @param values An object containing key-value pairs to set.
     * @param scope The configuration scope to retrieve the key from. 
     * Can be `'task'` for task-specific settings or `'project'` for project-wide settings.
     */
    setValues(values: any, scope: ConfigScope = 'task') {
        if (scope == 'task') {
            const entries = Object.entries(values)
            for (const [key, value] of entries) {
                this.set(key, value)
            }
            return
        }

        const entries = Object.entries(values)
        for (const [key, value] of entries) {
            this.set(key, value, 'project')
        }
    }

    /**
     * Deletes a task-specific configuration value for the current task.
     * @param scope The configuration scope to retrieve the key from. 
     * Can be `'task'` for task-specific settings or `'project'` for project-wide settings.
     * @param key The key to delete.
     */
    delete(key: string, scope: ConfigScope = 'task') {
        if (scope == 'task') {
            const obj = this.values?.tasks?.[this.forge.taskName]
            if (!obj) {
                return
            }

            if (!(key in obj)) {
                return
            }

            this.pendingChanges = true
            delete obj[key]
            return
        }

        const obj = this.values?.project
        if (!obj) {
            return
        }

        if (!(key in obj)) {
            return
        }

        this.pendingChanges = true
        delete obj[key]
    }

    /**
     * Checks whether there are unsaved changes in the config.
     * @returns True if there are pending changes, false otherwise.
     */
    hasPendingChanges() {
        return this.pendingChanges
    }

    /**
     * Gets the path to the current config file, if loaded or saved.
     * @returns The absolute path to the config file, or undefined if not set.
     */
    getConfigPath() {
        return this.configPath
    }

    /**
     * Creates a new empty config instance linked to the same Forge instance.
     * @returns A new ForgeConfig object.
     */
    newConfig() {
        return new ForgeConfig(this.forge)
    }

    /**
     * Saves the current config state to disk.
     * If the config path is not yet set, it will be set to the forge's targetPath.
     */
    async save() {
        this.pendingChanges = false
        this.configPath ??= this.forge.paths.targetPath(ConfigFileName)

        const dir = dirname(this.configPath)

        await fs.ensureDir(dir)
        await fs.writeJSON(this.configPath, this.values, {
            spaces: 2
        })
    }

    /**
     * Loads configuration from disk by searching for the config file in the given directory or one of its parent directories.
     * 
     * @param directory Optional directory to start the search from. Defaults to the Forge target path.
     * @returns True if a config file was found and loaded, false otherwise.
     */
    async load(directory?: string) {
        const targetDir = directory ?
            this.forge.paths.targetPath(directory) :
            this.forge.paths.targetPath()

        if (!await fs.exists(targetDir)) {
            return false
        }

        const filePath = await this.getConfigPathInternal(targetDir)
        if (!filePath) {
            return false
        }

        this.values = await fs.readJSON(filePath)
        this.configPath = filePath

        return true
    }

    /**
     * Recursively searches for the config file starting from the given directory
     * and moving up the directory tree until found or the root is reached.
     * 
     * @param directory The directory to start the search from.
     * @returns The full path to the config file if found, otherwise undefined.
     */
    private async getConfigPathInternal(directory: string): Promise<string | undefined> {
        const filePath = join(directory, ConfigFileName)

        if (await fs.exists(filePath))
            return filePath

        const parent = dirname(directory)

        if (parent == directory)
            return undefined

        return await this.getConfigPathInternal(parent)
    }
}