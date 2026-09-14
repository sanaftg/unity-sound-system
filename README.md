# FTG Sound System

毎回ゲームごとに作り直しがちな音声管理を共通化する、Unity向けの軽量サウンド管理パッケージです。

## Features

- AudioMixer: Master / BGM / SE / Jingle / Ambience / Voice
- BGM crossfade using two AudioSources
- Loop and non-loop repeat with a random interval
- Pooled one-shot playback with per-sound concurrency limits
- PlayerPrefs volume persistence
- ScriptableObject catalog with direct AudioClip references
- Persistent service across scene changes
- No external package dependencies

## Requirements

- Unity 2022.3 or newer

## Installation

Open **Window > Package Manager > + > Add package from git URL** and enter:

```text
https://github.com/sanaftg/unity-sound-system.git
```

After releases are tagged, pin a version:

```text
https://github.com/sanaftg/unity-sound-system.git#v0.1.0
```

## Setup

1. Create an AudioMixer in the game project.
2. Create the desired groups, typically Master, BGM, SE, Jingle, Ambience and Voice.
3. Expose each group's volume parameter.
4. Create **Assets > Create > FTG > Sound System > Sound Catalog**.
5. Add sound entries with unique string keys.
6. Create **Assets > Create > FTG > Sound System > Settings**.
7. Assign the catalog, mixer, output groups and exposed parameter names.
8. Add `SoundSystemBootstrap` to the first scene and assign the settings asset.

The package intentionally does not contain an AudioMixer asset or game-specific audio clips.

## Usage

```csharp
using Ftg.SoundSystem;

SoundService.Instance.PlayBgm("music.lobby");
SoundService.Instance.PlayOneShot("ui.card.select");
SoundService.Instance.PlayAmbience("ambience.wind");

SoundService.Instance.SetVolume(SoundChannel.Master, 1f);
SoundService.Instance.SetVolume(SoundChannel.Bgm, 0.8f);
SoundService.Instance.StopBgm(1f);
```

### Playback modes

- `OneShot`: plays once. Intended for SE, jingles and voices.
- `Loop`: loops seamlessly using `AudioSource.loop`.
- `RepeatWithInterval`: plays the full clip, waits for a random configured interval, then starts again.

BGM entries must use the BGM channel and ambience entries must use the Ambience channel.

## Design notes

The public playback API works with sound keys rather than direct clips. Version 0.1 uses a direct-reference `SoundCatalog`; loading can later be moved behind a provider (for example Addressables) without changing game call sites.

AudioMixer assets and all licensed audio remain in the consuming game project.

## License

MIT
