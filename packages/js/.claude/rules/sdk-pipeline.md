# SynkVault SDK — CI/CD Pipeline Guide

## Overview

The SDK uses a monorepo structure with an automated npm publishing pipeline. All version management, testing, and publishing is orchestrated through GitHub Actions.

**Pipeline File**: `/.github/workflows/publish-sdk.yml`

---

## Key Rules for Agents

### 1. Version Bumping (Required on Master PRs)

**When**: Every PR to `master` must include a version bump in `packages/js/package.json`

**Why**: The pipeline enforces semantic versioning via the `check-version` job — PRs without version bumps are automatically rejected.

**How to apply**:
- Check the current version in `packages/js/package.json`
- Update it following semantic versioning: `major.minor.patch[-prerelease]`
- For alpha releases: `0.1.0-alpha.X`
- For GA releases: `1.0.0`, `1.1.0`, etc.

**Exception**: PRs to `dev` branch do NOT require version bumps.

---

### 2. Monorepo Structure

**Why**: The repo now contains multiple SDKs (JS/TS, .NET). Publishing must target only `@synkvault/sdk` (the JS/TS package).

**How to apply**:
- JavaScript/TypeScript SDK is at `packages/js/` with package name `@synkvault/sdk`
- The root `package.json` is `"private": true` — it's a workspace coordinator, not a publishable package
- When checking versions in CI jobs, always read from `packages/js/package.json`, not the root
- When publishing, always use `--filter @synkvault/sdk` to target only the JS package

---

### 3. Build Artifacts

**Location**: `packages/js/dist/`

**Contents**:
- `index.js` — ESM bundle (+ `.js.map` source map)
- `index.cjs` — CommonJS bundle (+ `.cjs.map` source map)  
- `index.d.ts` — TypeScript declarations
- `index.d.cts` — TypeScript declarations for CommonJS

**Publishing**: Only the `dist/` directory (plus `package.json` and `README.md`) is published to npm. Build happens automatically via the `prepublishOnly` hook.

**Important**: Never commit `dist/` to git — it's generated on build. The `.gitignore` already excludes it.

---

### 4. Test & Type Check Requirements

**Before Publishing**:
1. All unit tests must pass (`pnpm test`)
2. TypeScript must have no errors (`pnpm typecheck`)
3. Build must succeed (`pnpm build`)

These are enforced by:
- The `test` job (runs on all PRs)
- The `publish` job (runs on master push after successful tests)

**How to apply**: Always run locally before pushing:
```bash
pnpm test
pnpm typecheck
pnpm build
```

---

### 5. Publishing Workflow

**Automated**: Publishing happens automatically when a PR is merged to `master`.

**Trigger**: The `publish` job runs on `push` to `master` only — it:
1. Installs dependencies
2. Runs all tests
3. Runs type check
4. Configures npm auth from `NPM_TOKEN` secret (GitHub environment: `sdk`)
5. Publishes via `pnpm publish --no-git-checks --filter @synkvault/sdk`

**Manual Publishing** (if needed):
```bash
cd packages/js
pnpm build
pnpm typecheck
npm publish
```

**Important**: Never force-push to `master` after merge — it will trigger a publish of potentially incomplete code.

---

### 6. Common Pitfalls

#### ❌ Reading root `package.json` for version checks
The root `package.json` has no `version` field. Always use `packages/js/package.json`.

#### ❌ Publishing from wrong directory
Running `pnpm publish` at the monorepo root will fail because root `package.json` is private. Use `--filter @synkvault/sdk` or `cd packages/js && npm publish`.

#### ❌ Forgetting to bump version on master PRs
The `check-version` job will reject your PR. Always increment the version in `packages/js/package.json`.

#### ❌ Committing `dist/` to git
The build outputs are generated — they should never be committed. Let CI build and publish.

#### ❌ Modifying workflow files without testing
Changes to `.github/workflows/publish-sdk.yml` affect the entire publishing pipeline. Test locally first or use a test branch.

---

### 7. Debugging Pipeline Failures

**Version Check Failed**:
- Verify `packages/js/package.json` has a version greater than the base branch
- Confirm the version is valid semver format

**Test Failed**:
- Run `pnpm install && pnpm test` locally to reproduce
- Check `packages/js/src/__tests__/` for test files

**Publish Failed**:
- Verify `NPM_TOKEN` is set in the `sdk` GitHub environment
- Check npm account permissions for `@synkvault/sdk`
- Ensure version hasn't already been published (npm doesn't allow re-publishing same version)

**Type Check Failed**:
- Run `pnpm typecheck` locally
- Review TypeScript errors in output

---

## See Also

- [sdk-testing.md](sdk-testing.md) — Test philosophy and structure
- [sdk-feature-map.md](sdk-feature-map.md) — Cross-SDK feature parity matrix
- [packages/js/.claude/CLAUDE.md](../) — SDK development guide
