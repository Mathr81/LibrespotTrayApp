# librespot pour LibrespotTrayApp

`librespot.exe` n'est plus versionné dans le dépôt : il est compilé depuis les sources officielles
par `build.ps1` (en local comme dans la CI), à partir d'une version figée de
[librespot](https://github.com/librespot-org/librespot) (actuellement **v0.8.0**) et de quelques
correctifs ciblés pour Windows.

## Compiler

Prérequis : [Git](https://git-scm.com/), [Rust](https://rustup.rs/) et les
« Outils de génération Visual Studio » (charge de travail C++).

```powershell
./librespot/build.ps1
```

Le résultat est placé dans `librespot/bin/librespot.exe`, d'où le projet C# le copie
automatiquement à côté de `LibrespotTrayApp.exe`.

Options de compilation : LTO complète, une seule unité de génération de code, symboles retirés et
runtime C lié statiquement (aucun « Visual C++ Redistributable » nécessaire).

## Correctifs (`patches/`)

### `librespot.patch` — sortie audio (backend Rodio)

- **Qualité** : sous Windows, la sortie est ouverte directement au format natif de Spotify
  (stéréo, 44,1 kHz, flottant 32 bits). Si le périphérique tourne à une autre fréquence (48 kHz le
  plus souvent), c'est le moteur audio de Windows qui convertit, avec son rééchantillonneur de
  qualité, au lieu de l'interpolation linéaire de Rodio (qui dégrade les aigus).
- **Pas de saccades** : tampon WASAPI d'environ 93 ms au lieu d'environ 10 ms, et thread audio
  enregistré auprès du service MMCSS de Windows (tâche « Audio ») via la fonctionnalité
  `audio_thread_priority` de cpal. Sans elle, cpal tente de passer le thread en priorité critique
  mais passe un identifiant de thread là où Windows attend un handle : l'appel échoue et le thread
  reste en priorité normale.
- **Zéro CPU en pause** : la sortie audio n'est ouverte que pendant la lecture. En pause, le
  périphérique est libéré (Rodio continuerait sinon à mixer du silence en permanence), et la
  reprise utilise le périphérique par défaut *actuel* de Windows.
- **Robustesse** : si le périphérique disparaît pendant la lecture (casque débranché,
  périphérique désactivé, format modifié…), la sortie est rouverte automatiquement au lieu de
  bloquer librespot dans le silence. Si le périphérique configuré n'existe pas, le périphérique par
  défaut est utilisé.

### `cpal.patch` — cpal 0.16.0 (WASAPI)

Les flux WASAPI en mode partagé n'acceptent nativement que le format de mixage du moteur audio.
Pour les autres formats, le correctif active `AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM` et
`AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY` au lieu de refuser le flux.

## Mettre à jour librespot

1. Modifier `$LibrespotTag` (et si besoin `$CpalVersion`/`$CpalSha256`) dans `build.ps1`.
2. Régénérer les correctifs : cloner la nouvelle version, réappliquer les modifications, puis
   `git diff > patches/librespot.patch` (y compris le `Cargo.lock` mis à jour).
3. Vérifier les options passées par `LibrespotTrayApp/LibrespotService.cs` dans le `CHANGELOG.md`
   de librespot.
