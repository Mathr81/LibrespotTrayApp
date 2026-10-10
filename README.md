# ✨ Librespot Tray App ✨

[![Build](https://img.shields.io/github/actions/workflow/status/Mathr81/LibrespotTrayApp/build.yml?branch=main&label=Build&style=for-the-badge)](https://github.com/Mathr81/LibrespotTrayApp/actions/workflows/build.yml)
[![Latest release](https://img.shields.io/github/v/release/Mathr81/LibrespotTrayApp?style=for-the-badge&label=Latest%20Release)](https://github.com/Mathr81/LibrespotTrayApp/releases/latest)

---

## 🚀 Spotify, sans le client Spotify

Librespot Tray App transforme votre PC Windows en appareil **Spotify Connect** grâce à
[librespot](https://github.com/librespot-org/librespot), discrètement depuis la zone de
notification. Vous pilotez la musique depuis votre téléphone (ou n'importe quel appareil Spotify),
et votre PC la joue — sans faire tourner le client Spotify (Electron/Chromium) et ses centaines de
Mo de RAM.

Pensé pour **jouer en écoutant de la musique** : le moins de RAM et de CPU possible, une très bonne
qualité, et aucune saccade même quand le jeu occupe le processeur.

### Pourquoi c'est léger et sans saccades

| | |
|---|---|
| **Qualité maximale** | 320 kbit/s, sortie en flottant 32 bits (pas de conversion ni de dithering) ; si votre carte son tourne à 48 kHz, c'est le rééchantillonneur haute qualité de Windows qui convertit, pas une interpolation linéaire. |
| **Anti-saccades** | Thread audio enregistré auprès de MMCSS (comme les lecteurs audio pro), tampon audio plus large, priorité « supérieure à la normale » par défaut, et librespot exclu du mode « efficacité énergétique » (EcoQoS) de Windows 11, qui provoque des coupures. |
| **0 % CPU en pause** | La carte son est libérée quand la musique est en pause. |
| **App tray minuscule** | Mode efficacité (EcoQoS) et priorité mémoire basse pour l'app tray elle-même, mémoire rendue à Windows quand elle ne sert pas : quelques Mo de RAM en jeu. |
| **Fiable** | librespot est relancé automatiquement s'il s'arrête (avec temporisation), après une mise en veille, et se reconnecte tout seul si vous débranchez le casque. Il ne survit jamais à l'app tray (même en cas de plantage). |

### Fonctionnalités

* Connexion à Spotify au premier lancement (page de connexion ouverte dans le navigateur), puis
  identifiants mémorisés.
* Paramètres : nom et type de l'appareil, qualité, périphérique de sortie, normalisation du volume,
  volume de départ ou reprise du dernier volume, autoplay, priorité, taille et dossier du cache
  audio, visibilité sur le réseau local, journal détaillé, déconnexion du compte.
* Lancement au démarrage de Windows, journal consultable en un clic.
* Une seule instance : relancer l'application ouvre ses paramètres.

---

## 📥 Installation

1. Téléchargez `LibrespotTrayApp-Installer-vX.Y.Z.exe` depuis la
   [dernière release](https://github.com/Mathr81/LibrespotTrayApp/releases/latest).
2. Lancez-le : il met à jour une version précédente en place (paramètres et connexion conservés).
3. Au premier lancement, connectez-vous à Spotify dans la page qui s'ouvre.

Windows 10 ou 11 64 bits. Un compte **Spotify Premium** est nécessaire (exigence de Spotify pour
les appareils Connect).

## 🛠️ Utilisation

* **Double-clic** sur l'icône : paramètres.
* **Clic droit** : état de la connexion, paramètres, redémarrage de librespot, démarrage avec
  Windows, journal, quitter.
* L'icône est **verte** quand l'appareil est prêt, **grise** sinon.

Puis dans Spotify (téléphone, web, etc.) : icône des appareils → choisissez votre PC.

### Astuce qualité audio

Pour un chemin audio sans aucun rééchantillonnage, réglez le format de votre sortie audio sur
**44 100 Hz** (Paramètres Windows → Son → votre périphérique → Format). C'est facultatif : à 48 kHz,
la conversion est faite par Windows avec une bonne qualité.

## 🗂️ Fichiers

Tout est dans `%LocalAppData%\LibrespotTrayApp` : `config.json` (paramètres), `librespot.log`
(journal de la session, `librespot.previous.log` pour la précédente), `session\` (identifiants
Spotify et dernier volume) et `cache\` (cache audio, limité à 2 Go par défaut).

---

## 🧑‍💻 Développement

* `LibrespotTrayApp/` : application C# (.NET 10, WinForms).
* `librespot/` : compilation de `librespot.exe` depuis les sources (voir
  [librespot/README.md](librespot/README.md)).
* `Installer/` : script Inno Setup.

```powershell
./librespot/build.ps1                     # compile librespot.exe (Rust requis)
dotnet build LibrespotTrayApp -c Release  # ou ouvrez LibrespotTrayApp.sln
```

La CI (`.github/workflows/build.yml`) compile tout et produit l'installateur à chaque push. Pour
publier une release : onglet **Actions** → **Build** → **Run workflow** sur `main` en indiquant la
version (par ex. `2.0.1`), ou poussez un tag `vX.Y.Z`.

## 🤝 Contribuer

Les [issues](https://github.com/Mathr81/LibrespotTrayApp/issues) et
[pull requests](https://github.com/Mathr81/LibrespotTrayApp/pulls) sont les bienvenues.
