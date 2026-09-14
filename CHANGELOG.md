# Changelog

All notable changes to this package are documented in this file.

## [0.1.1] - 2026-09-14

### Fixed

- Fixed CS0177 when a null or whitespace sound key is checked.
- Added the missing LICENSE meta file for immutable Git package imports.
- Repeated requests for the active BGM no longer restart it by default.

## [0.1.0] - 2026-09-14

### Added

- UPM-compatible package structure.
- Persistent sound service and bootstrap component.
- AudioMixer channel bindings and PlayerPrefs volume persistence.
- Two-source BGM crossfading.
- Loop and repeat-with-random-interval playback modes.
- Pooled one-shot playback with per-sound concurrency limits.
- Ambience playback and fading.
- Direct-reference sound catalog.
- Edit Mode tests and setup documentation.
