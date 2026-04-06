# SDK Extraction Notes

## What Happened

The `@synkvault/sdk` package has been extracted from the `synkvault-web` monorepo into its own independent repository.

**New Location**: `/Users/ahmed/Code/CyberneticLabs/sdk`  
**GitHub**: `cybernetic/synkvault-sdk` (to be created)

## Timeline

- **Old**: `synkvault-web/packages/js/sdk/`
- **New**: `/Users/ahmed/Code/CyberneticLabs/sdk/`

Both changes are committed:
- SDK repo initial commit: `42ad46b`
- synkvault-web removal: commit `495b230`

## What Moved

All SDK files and configuration:
- `/src/**/*.ts` — Source code and tests
- `package.json`, `tsconfig.json`, `tsup.config.ts`, `vitest.config.ts`
- `.npmrc`, `.gitignore`
- `README.md` — Developer guide
- `.claude/CLAUDE.md` — Project rules
- `.claude/rules/sdk-testing.md` — Testing guidelines

## What Changed in synkvault-web

- Removed: `packages/js/sdk/` directory
- Removed: `.claude/rules/sdk-testing.md`
- Updated: `package.json`
  - `sdk:build` → references `../../sdk` instead of `packages/js/sdk`
  - `sdk:publish` → references `../../sdk` instead of `packages/js/sdk`

## Development Workflow

### Build the SDK

```bash
cd /Users/ahmed/Code/CyberneticLabs/sdk
pnpm install
pnpm build          # One-time
pnpm build:watch    # Watch mode
```

### Test the SDK

```bash
pnpm test           # Run once
pnpm test:watch     # Watch mode
pnpm test:coverage  # With coverage
```

### From synkvault-web (if needed)

```bash
# Build SDK (from synkvault-web root)
pnpm sdk:build

# Publish SDK (from synkvault-web root)
pnpm sdk:publish
```

## Publishing

The SDK is independently versioned and published to npm as `@synkvault/sdk`.

To publish a new version:
1. Update version in `/Users/ahmed/Code/CyberneticLabs/sdk/package.json`
2. Commit the version bump
3. Run `pnpm publish` (or `pnpm sdk:publish` from synkvault-web)

## Next Steps

- [ ] Create GitHub repo: `cybernetic/synkvault-sdk`
- [ ] Push SDK repo to GitHub
- [ ] Update CI/CD to build SDK independently
- [ ] Publish SDK to npm (first release or use existing account)
- [ ] Update synkvault-web CI to reference SDK by npm package instead of local path (optional — local path still works)
