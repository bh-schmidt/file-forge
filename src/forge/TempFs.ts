import fs from 'fs-extra';
import { dirname, join, resolve } from "path";
import { Database, open } from 'sqlite';
import sqlite3 from 'sqlite3';
import { GlobHelper } from '../common/GlobHelper';
import { PathHelper } from "../common/PathHelper";
import { IFileInjector } from "../Injectors/LiquidInjector";
import { FileExistsAction, TempPathType, WriteOptions } from "../types";

export interface TempPath {
    targetPath: string
    type?: TempPathType
    tempPath?: string
    ifFileExists?: FileExistsAction
    approved?: boolean
}

export class TempFs {
    private db: Database | undefined = undefined
    private dbPath: string

    constructor(private tempDir: string, private injector: IFileInjector) {
        this.dbPath = join(this.tempDir, 'index.db')
    }

    async getConnection() {
        if (this.db)
            return this.db

        this.db = await open({
            driver: sqlite3.Database,
            filename: this.dbPath
        })

        await this.db.exec(`
            CREATE TABLE IF NOT EXISTS temp_index (
                target_path TEXT NOT NULL,
                type TEXT NOT NULL,
                temp_path TEXT,
                if_file_exists TEXT,
                approved INTEGER,
                PRIMARY KEY (target_path, type)
            );

            CREATE INDEX IF NOT EXISTS idx_temp_index_target_path ON temp_index(target_path);

            CREATE INDEX IF NOT EXISTS idx_temp_index_type ON temp_index(type);
            `)

        return this.db
    }

    async getTempPath(path: string, type: TempPathType) {
        path = resolve(path)

        const con = await this.getConnection()

        return await con.get<TempPath>(`
            SELECT 
                target_path targetPath,
                temp_path tempPath,
                if_file_exists ifFileExists,
                approved approved
            FROM temp_index
            WHERE 1=1
                AND target_path = :target
                AND type = :type`,
            {
                ':target': path,
                ':type': type
            })
    }

    async* getTempPaths(filters?: Partial<TempPath>, sort: boolean = false) {
        filters ??= {}

        if (filters?.targetPath) {
            filters.targetPath = resolve(filters.targetPath)
        }

        const con = await this.getConnection()

        const pathFilter = filters?.targetPath ?
            'AND target_path = :target' :
            ''

        const typeFilter = filters?.type ?
            'AND type = :type' :
            ''

        const approvedFilter = filters?.approved ?
            'AND approved = :approved' :
            ''

        const orderBy = sort ?
            'ORDER BY LENGTH(target_path) ASC, target_path ASC' :
            ''

        let offset = 0
        const pageSize = 100

        while (true) {
            const rows = await con.all<TempPath[]>(`
                SELECT 
                    target_path targetPath,
                    temp_path tempPath,
                    if_file_exists ifFileExists,
                    approved approved
                FROM temp_index
                WHERE 1=1
                    ${pathFilter}
                    ${typeFilter}
                    ${approvedFilter}
                ${orderBy}
                LIMIT :limit OFFSET :offset
                `,
                {
                    ':target': filters?.targetPath,
                    ':type': filters?.type,
                    ':approved': filters?.approved,
                    ':limit': pageSize,
                    ':offset': offset,
                })

            if (rows.length == 0) {
                return
            }

            offset += pageSize

            for (const row of rows) {
                yield row
            }
        }
    }

    async approve(path: string) {
        const con = await this.getConnection()
        await con.run(
            `UPDATE temp_index SET approved = 1 WHERE target_path = :path`,
            {
                ':path': path
            }
        )
    }

    private async setTempFile(tempFile: TempPath) {
        tempFile.targetPath = resolve(tempFile.targetPath)
        tempFile.tempPath = resolve(tempFile.tempPath!)
        tempFile.type = 'file'

        const current = await this.getTempPath(tempFile.targetPath, 'file')
        if (current) {
            await fs.rm(current.tempPath!)
        }

        const con = await this.getConnection()
        await con.run(`
            INSERT OR REPLACE INTO temp_index
                (target_path, type, temp_path, if_file_exists)
            VALUES
                (:target, :type, :temp, :file_exists)
            `,
            {
                ':target': tempFile.targetPath,
                ':type': tempFile.type,
                ':temp': tempFile.tempPath,
                ':file_exists': tempFile.ifFileExists
            })
    }

    private async setTempDir(tempDir: TempPath) {
        tempDir.targetPath = resolve(tempDir.targetPath)
        tempDir.type = 'directory'

        const con = await this.getConnection()
        await con.run(`
            INSERT OR REPLACE INTO temp_index
                (target_path, type, approved)
            VALUES
                (:target, :type, :approved)
            `,
            {
                ':target': tempDir.targetPath,
                ':type': tempDir.type,
                ':approved': tempDir.approved
            })
    }

    private async newTempPath() {
        while (true) {
            const tempPath = join(this.tempDir, crypto.randomUUID() + '.temp')
            if (await fs.exists(tempPath)) {
                continue
            }

            return tempPath
        }
    }

    async ensureDirectory(path: string) {
        const temp: TempPath = {
            targetPath: path,
            approved: true
        }

        await this.setTempDir(temp)
    }

    async writeFile(path: string,
        data: string | NodeJS.ArrayBufferView,
        options?: WriteOptions,
    ) {
        await this.ensureDirectory(dirname(path))

        const temp: TempPath = {
            tempPath: await this.newTempPath(),
            targetPath: path,
            ifFileExists: options?.ifFileExists
        }

        await fs.writeFile(temp.tempPath!, data)
        await this.setTempFile(temp)
    }

    async copyFile(src: string, dest: string, options: WriteOptions = {}) {
        const stat = await fs.stat(src)
        if (stat.isDirectory()) {
            throw new Error('src is a directory')
        }

        await this.ensureDirectory(dirname(dest))

        const temp: TempPath = {
            tempPath: await this.newTempPath(),
            targetPath: dest,
            ifFileExists: options?.ifFileExists
        }

        await fs.copy(src, temp.tempPath!)
        await this.setTempFile(temp)
    }

    async injectFile(src: string, dest: string, variables: any, options: WriteOptions = {}) {
        const stat = await fs.stat(src)
        if (stat.isDirectory()) {
            throw new Error('src is a directory')
        }

        const newPath = PathHelper.injectPath(dest, variables)
        const newDir = dirname(newPath)
        await this.ensureDirectory(newDir)

        const temp: TempPath = {
            tempPath: await this.newTempPath(),
            targetPath: newPath,
            ifFileExists: options?.ifFileExists
        }

        await this.setTempFile(temp)

        if (await this.injector.shouldInject(src, temp.tempPath!, variables)) {
            await this.injector.inject(src, temp.tempPath!, variables)
            return
        }

        await fs.copyFile(src, temp.tempPath!)
    }

    async exists(path: string) {
        const iterator = this.getTempPaths({
            targetPath: path
        })
        const result = await iterator.next()

        return !result.done
    }

    async readFile(path: string) {
        const tempPath = await this.getTempPath(path, 'file')
        if (!tempPath) {
            return undefined
        }

        const tempFilePath = join(this.tempDir, tempPath.tempPath!)
        return await fs.readFile(tempFilePath)
    }

    async createReadStream(path: string, options?: BufferEncoding) {
        const tempPath = await this.getTempPath(path, 'file')
        if (!tempPath) {
            throw new Error(`File '${path}' does not exist in the temp directory.`)
        }

        const tempFilePath = join(this.tempDir, tempPath.tempPath!)
        return fs.createReadStream(tempFilePath, options)
    }

    async rollback() {
        if (this.db) {
            this.db.close()
        }

        await this.clearFiles()
    }

    async commit() {
        const con = await this.getConnection()

        const directories = this.getTempPaths(
            {
                type: 'directory'
            },
            true)

        for await (const dir of directories) {
            await fs.ensureDir(dir.targetPath)
        }

        const files = this.getTempPaths(
            {
                type: 'file',
                approved: true
            },
            true)

        for await (const file of files) {
            await fs.copy(file.tempPath!, file.targetPath)
            await fs.rm(file.tempPath!)
        }

        await con.close()
        await this.clearFiles()
    }

    private async clearFiles() {
        const tempFiles = GlobHelper.globAll(
            '**/*.temp',
            {
                cwd: this.tempDir,
                nodir: true,
                absolute: true
            })

        for await (const file of tempFiles) {
            await fs.rm(file)
        }

        if (await fs.exists(this.dbPath)) {
            await fs.rm(this.dbPath)
        }
    }
}