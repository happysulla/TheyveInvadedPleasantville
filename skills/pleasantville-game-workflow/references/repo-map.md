# Repo Map

## Main project

- `PleasantvilleGame/Model`: Core gameplay state, game rules, players, map items, and config readers.
- `PleasantvilleGame/View`: WPF dialogs, viewers, and code-behind for the visible game flow.
- `PleasantvilleGame/Interfaces`: Contracts that mirror many model types and help reveal intended boundaries.
- `PleasantvilleGame/Networking`: Multiplayer DTOs, protobuf mapping, snapshots, and state application.
- `PleasantvilleGame/Config`: XML and text files that drive game content and outcomes.
- `PleasantvilleGame/UnitTests`: Numbered tests plus helper dialogs used by the current test setup.

## Common change patterns

- Gameplay logic changes often touch `Model/`, a matching interface in `Interfaces/`, and a focused test in `UnitTests/`.
- UI changes often require checking both `*.xaml` and `*.xaml.cs`, plus any model objects they bind to or manipulate.
- Multiplayer changes should be reviewed across DTOs, mappers, snapshot creation, and state application to avoid partial protocol drift.
- Config-driven changes may require both the relevant file in `Config/` and content settings in `PleasantvilleGame.csproj`.

## Watchouts

- `PleasantvilleGame.csproj` contains long content lists for images and config files. Edit carefully and avoid broad reshuffling.
- Multiplayer input should be treated as external and untrusted even if it originates from another game client.
- The test layout is older and file names are numbered, so prefer narrow additions over wholesale test reorganizations.
