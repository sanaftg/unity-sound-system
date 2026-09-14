# Basic Usage

1. Create a `Sound Catalog` from **Assets > Create > FTG > Sound System > Sound Catalog**.
2. Add unique keys and clips to the catalog.
3. Create `Sound System Settings` and assign the catalog, AudioMixer and mixer bindings.
4. Add `SoundSystemBootstrap` to the first scene and assign the settings asset.

```csharp
SoundService.Instance.PlayBgm("music.lobby");
SoundService.Instance.PlayOneShot("ui.confirm");
SoundService.Instance.PlayAmbience("ambience.wind");
SoundService.Instance.SetVolume(SoundChannel.Bgm, 0.8f);
```
