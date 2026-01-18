import { spawnSync } from "node:child_process";
import path from "node:path";

function run(cmd, args, options = {}) {
  const result = spawnSync(cmd, args, {
    stdio: "inherit",
    shell: false,
    ...options,
  });

  if (result.error) {
    throw result.error;
  }
  if (typeof result.status === "number" && result.status !== 0) {
    throw new Error(`${cmd} ${args.join(" ")} exited with code ${result.status}`);
  }
}

const here = path.dirname(new URL(import.meta.url).pathname);
const frontendRoot = path.resolve(here, "..");
const repoRoot = path.resolve(frontendRoot, "..", "..");
const montiCoreToolDir = path.resolve(repoRoot, "MontiCoreTool");

console.log("=== VERIFY: GRAMMAR ↔ FRONTEND SYNC ===");
console.log("1) Validating MontiCore grammars via generateMCGrammars...");
run("gradle", ["generateMCGrammars", "--no-daemon"], { cwd: montiCoreToolDir });

console.log("2) Regenerating frontend grammar artifacts...");
run("node", ["./scripts/gen-grammar.mjs"], {
  cwd: frontendRoot,
  env: {
    ...process.env,
    FORCE_REGEN: "1",
  },
});

console.log("3) Checking that generated artifacts are committed/up-to-date...");
run(
  "git",
  [
    "diff",
    "--exit-code",
    "-I",
    "^// Generated at:",
    "-I",
    "\"generatedAt\"",
    "--",
    "src/generated",
  ],
  { cwd: frontendRoot }
);

console.log("✓ Grammar is valid (MontiCore) and frontend artifacts are in sync.");
