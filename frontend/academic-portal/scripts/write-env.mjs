import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const targetPath = path.resolve(__dirname, "../public/env.js");
const apiBaseUrl = process.env.API_BASE_URL ?? "http://localhost:5262";

const content = `window.__env = window.__env || {};
window.__env.API_BASE_URL = "${apiBaseUrl}";
`;

fs.writeFileSync(targetPath, content, { encoding: "utf8" });
console.log(`[write-env] API_BASE_URL=${apiBaseUrl}`);
