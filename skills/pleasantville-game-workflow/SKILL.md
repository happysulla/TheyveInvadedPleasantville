---
name: pleasantville-game-workflow
description: Use this skill when working in the They've Invaded Pleasantville repo to implement or review C# WPF gameplay, multiplayer, config-driven content, or unit tests.
---

# Pleasantville Game Workflow

Use this skill for repo-specific work in `PleasantvilleGame`.

## When to use it

- Gameplay or state changes in `Model/`
- WPF UI changes in `View/` or `MainWindow.xaml.cs`
- Multiplayer or serialization changes in `Networking/`
- Config or asset-driven behavior changes in `Config/` and the project file
- Test updates in `UnitTests/`

## Workflow

1. Identify the change area before editing. If the scope is unclear, run `scripts/inventory-area.ps1`.
2. Read [references/repo-map.md](references/repo-map.md) for the likely folder owners and common cross-file dependencies.
3. Keep diffs small and explicit. Avoid unrelated cleanup in `PleasantvilleGame.csproj`, especially around the long content lists.
4. For gameplay changes, inspect the corresponding interface in `Interfaces/` and any config files the logic reads.
5. For UI changes, review both the XAML and its code-behind, then confirm any model or command contracts still line up.
6. For multiplayer changes, treat all inbound data as untrusted. Validate nullability, ranges, and state transitions before applying them. Do not leak raw exceptions or internal details to users.
7. Update or add focused tests under `UnitTests/` when behavior changes. If tests cannot be run, say so explicitly.

## Security guardrails

- Prefer managed-memory .NET patterns and explicit validation over clever parsing.
- Do not log secrets, session identifiers, or full payloads from multiplayer traffic.
- Use generic user-facing errors and keep detailed diagnostics internal.
- Default to deny or ignore malformed external input rather than trying to recover silently.

## Bundled resources

- `scripts/inventory-area.ps1`: Lists likely files for a requested area.
- `references/repo-map.md`: Quick map of the repo's main folders and hotspots.

