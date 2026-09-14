# FTG Sound System

毎回ゲームごとに作り直しがちな音声管理を共通化する、Unity向けの軽量サウンド管理パッケージです。

## Features

- AudioMixer: Master / BGM / SE / Jingle / Ambience / Voice
- BGM crossfade using two AudioSources
- Loop and non-loop repeat with a random interval
- Pooled 2D, positioned 3D and Transform-following one-shot playback
- Reusable Spatial Profile assets with a project-wide default
- SoundEmitter component for Inspector, UnityEvent and Animation Event playback
- Catalog Inspector tool for batch-registering selected AudioClips
- Per-sound concurrency limits
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
https://github.com/sanaftg/unity-sound-system.git#v0.1.2
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
SoundService.Instance.PlayOneShot("ui.card.select"); // 2D
SoundService.Instance.PlayOneShotAt("se.sword.hit", hitPosition); // positioned 3D
SoundService.Instance.PlayOneShotAttached("voice.attack", unitTransform); // following 3D
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

## Spatial audio

Create reusable profiles from **Assets > Create > FTG > Sound System > Spatial Profile**. Assign the usual profile to Settings as the default, then omit it at call sites. Pass a different profile only for near or far sounds. The catalog does not store whether a sound is 2D or 3D; the playback API decides that.

```csharp
SoundService.Instance.PlayOneShotAt("se.footstep", position, nearProfile);
SoundService.Instance.PlayOneShotAt("se.explosion", position, farProfile);
```

Add `SoundEmitter` to a GameObject when playback should be triggered from the Inspector, UnityEvent or Animation Event. Its `Play()` method supports 2D, positioned and attached playback.

## Catalog registration

Select multiple AudioClip assets and open their SoundCatalog. In the Catalog Inspector, choose a channel and click **Add Selected AudioClips**. Keys are generated from the channel and clip name, and made unique without adding 2D/3D state to the catalog.
