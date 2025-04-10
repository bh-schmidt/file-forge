import { TempDir } from "./common/TempDir";
import { createForge } from "./forge/Forge";

export const tempDir = TempDir.get()
export const executionsTempDir = TempDir.get("executions")

export { createForge }