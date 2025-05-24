# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.5.1] - 24/05/2025

### Fixed
- Fixed an issue during lobby creation if the number of installed mods were under a certain amount.

## [1.5.0] - 11/05/2025

### Added

- Ability to disable the \[MOD] prefix in the config (thanks [@baerchen201](https://github.com/baerchen201)!)
- Increased logging for debugging purposes when failing to connect to lobby.

### Fixed
- Fixed a connection issue when many mods were installed on a lobby. Because of this, there will be cases where not all mods a visible to a client, but they are prioritized in compatibility requirements - reducing the impact of this change.

## [1.4.0] - 03/02/2025

### Added

- Variable Compatibility
  - Adds the ability to change the compatibility of a mod based off the lobby settings. Defaults to ClientOnly compatibility.

## [1.3.0] - 06/11/2024

### Changed

- Disable certain functionality on LAN | authored by @1A3Dev

### Fixed

- Fix LAN-related issues | authored by @1A3Dev

## [1.2.0] - 20/08/2024

### Changed

- Changed internal method to hide the lobby from vanilla clients.

### Fixed

- Fixed the fatal error introduced in Lethal Company v62 that caused the mod to not function correctly.

## [1.1.0] - 01/04/2024

### Added

- Prevent vanilla clients from seeing modded lobbies in the lobby list.

## [1.0.4] - 30/03/2024

### Fixed

- Fixed issue with unloaded plugins.

## [1.0.3] - 24/03/2024

### Fixed

- Fixed incorrect LobbyCompatibility spelling in Thunderstore.toml (@1A3Dev)

## [1.0.2] - 23/03/2024

### Fixed

- Fixed manually registered plugins not being filtered from the auto-discovered list of plugins, and hence having an "unknown" duplicate entry.

## [1.0.1]

### Changed

- Changed Thunderstore Icon to be 256x256 to fit Thunderstore Upload Reqs

## [1.0.0]

### Added

- Mod compatibility information
- Block incompatible clients from joining lobbies
- Modded leaderboard
- `LobbyCompatibility` attribute
    - `CompatibilityLevel` Enum
    - `VersionStrictness` Enum
- register method for easier soft dependencies or dynamic registration
- Lobby browser UI with diff information
