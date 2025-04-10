import { promises } from "fs"
import fs from 'fs-extra'
import { isText } from "istextorbinary"
import { Liquid } from "liquidjs"

export interface IFileInjector {
    shouldInject(srcPath: string, targetPath: string, variables: any): boolean | Promise<boolean>
    inject(srcPath: string, targetPath: string, variables: any): void | Promise<void>
    getInjection(content: string, variables: any): string | Promise<string | Buffer>
}

export class LiquidInjector implements IFileInjector {
    async shouldInject(srcPath: string, _: string, _2: any): Promise<boolean> {
        const textExtension = isText(srcPath)
        if (textExtension) {
            return textExtension
        }

        const buffer = Buffer.alloc(8 * 1024)
        const fh = await promises.open(srcPath, 'r')
        try {
            await fh.read(buffer, 0, buffer.length, 0)
            return isText(undefined, buffer) ?? false
        } finally {
            await fh.close()
        }
    }

    async inject(srcPath: string, targetPath: string, variables: any): Promise<void> {
        const buffer = await fs.readFile(srcPath)
        const content = buffer.toString()

        const result = await this.getInjection(content, variables)
        await fs.writeFile(targetPath, result)
    }

    async getInjection(content: string, variables: any): Promise<string | Buffer> {
        const liquid = new Liquid()
        return await liquid.parseAndRender(content, variables)
    }
}