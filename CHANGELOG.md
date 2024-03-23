# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
