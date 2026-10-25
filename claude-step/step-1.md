# SpeedX 3D — Devlog

## Setup ambiente

- Engine scelto: **Godot 4** con **C#** — più stabile su Arch Linux rispetto a Unity
- Installato `godot-mono` su Arch Linux con supporto Wayland (`godot-mono --display-driver wayland`)
- Editor esterno: **Rider** (JetBrains) per C# con IntelliSense completo
- Renderer: **Mobile** — ottimo bilanciamento per giochi 3D mobile
- Deploy testato su **Samsung S24+** via ADB

---

## Struttura del progetto

```
res://
├── scenes/
│   ├── Main.tscn
│   ├── Player.tscn
│   ├── Chunk.tscn
│   ├── Obstacle.tscn
│   └── MusicPlayer.tscn
├── shaders/
│   └── trail.gdshader
└── audio/
    └── music.ogg
```

---

## Scene

| Scena | Nodo root | Descrizione |
|---|---|---|
| Main.tscn | Node3D | Scena principale |
| Player.tscn | CharacterBody3D | Il player |
| Chunk.tscn | Node3D | Sezione del tunnel |
| Obstacle.tscn | StaticBody3D | Ostacolo |
| MusicPlayer.tscn | AudioStreamPlayer | Musica (AutoLoad) |

---

## Script

### Player.cs

- Movimento in avanti con velocità presa da `GameManager.Instance.CurrentSpeed`
- Cambio corsia con frecce tastiera (`IsActionPressed`)
- Controllo accelerometro con zona morta (`TiltThreshold`) e sensibilità (`TiltSensitivity`)
- Rilevamento collisioni con `GetSlideCollisionCount` dopo `MoveAndSlide`
- Aggiunta al gruppo `"player"` nel `_Ready`
- 5 corsie: `-2, -1, 0, 1, 2` con `LaneWidth = 2.5f`
- `MaxX = 6.25f` per limitare il movimento ai bordi del tunnel

### ChunkSpawner.cs

- Generazione procedurale infinita del tunnel
- Spawn ostacoli con difficoltà progressiva — da 1 a 4 massimo
- `_lastChunkLanes` — memoria delle corsie dell'ultimo chunk per evitare muri impossibili
- Ostacoli spawnati nella seconda metà del chunk per dare spazio di manovra
- Chunk rimossi dalla coda quando troppo indietro rispetto al player

### GameManager.cs

- Singleton con `Instance`
- Velocità crescente da `StartSpeed` a `MaxSpeed` con `SpeedIncreaseRate`
- Punteggio basato sulla velocità × tempo
- Salvataggio `BestScore` persistente con `FileAccess` in `user://best_score.dat`
- Trigger game over con aggiornamento automatico del best score

### UI.cs

- `ScoreLabel` — punteggio in tempo reale
- `SpeedLabel` — velocità corrente
- `BestScoreLabel` — miglior punteggio
- `GameOverScreen` — schermata game over con best score
- Messaggio `"Nuovo record"` se il punteggio attuale supera il best
- Restart con `GetTree().ReloadCurrentScene()`

### CameraFollow.cs

- Segue il player con `Offset` configurabile dall'Inspector
- Oscillazione naturale con `Mathf.Sin` (`SwayAmount`, `SwaySpeed`)
- Inclinazione laterale in base al movimento del player (`TiltAmount`, `TiltSpeed`)
- Aggiornata in `_PhysicsProcess` per essere sincrona con il player

### Obstacle.cs

- Colori metallici casuali tra rosso `(0.9, 0.1, 0.1)`, blu `(0.1, 0.3, 0.9)`, verde `(0.1, 0.8, 0.1)`
- Materiale con `Metallic = 0.7`, `Roughness = 0.3`, emissione `0.4`
- Materiale duplicato con `Duplicate()` per evitare modifiche al materiale originale
- Scia davanti all'ostacolo generata via codice con `ShaderMaterial`

### MusicPlayer.cs

- Caricato come **AutoLoad** — persiste tra le scene senza resettarsi al restart
- Loop abilitato via codice su `AudioStreamOggVorbis`
- Non si ferma al game over — continua dall'punto in cui si trovava

---

## Shader

### trail.gdshader

Gradiente continuo per la scia davanti agli ostacoli:

```glsl
shader_type spatial;
render_mode blend_mix, depth_draw_never, unshaded, cull_disabled;

uniform vec4 trail_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);

void fragment() {
    float alpha = UV.y * UV.y;
    ALBEDO = trail_color.rgb;
    ALPHA = alpha * trail_color.a;
    EMISSION = trail_color.rgb * alpha;
}
```

- `UV.y * UV.y` — curva quadratica per gradiente naturale
- Colore passato dinamicamente da `Obstacle.cs` con `SetShaderParameter`

---

## Effetti visivi

- Pavimento chunk bianco tenue con luce ambientale (`Ambient Energy: 0.4`)
- 6 linee di corsia sul pavimento (emissione bianca/azzurra)
- 2 linee trasversali per chunk a 1/3 e 2/3 della lunghezza (`Z=-10` e `Z=-30`)
- Ostacoli metallici con colori casuali ed emissione
- Scia davanti agli ostacoli con shader gradiente continuo
- **Fog** nero per nascondere lo spawn degli ostacoli in lontananza
- Oscillazione e inclinazione camera in base al movimento

---

## Export Android

```bash
# Prerequisiti
sudo pacman -S jdk17-openjdk android-tools
yay -S android-sdk-cmdline-tools-latest
sdkmanager "platform-tools" "build-tools;34.0.0" "platforms;android-34"

# Keystore
keytool -genkey -v -keystore ~/mygame.keystore \
  -alias mygame -keyalg RSA -keysize 2048 -validity 10000

# Deploy
adb install --replace ~/speed-x-game-export/speedx.apk
```

Configurazione Godot:
- `Editor Settings → Export → Android → Android Sdk Path: /opt/android-sdk`
- `Project Settings → Input Devices → Sensors → Enable Accelerometer: ON`
- Export templates: `Godot_v4.6.1-stable_mono_export_templates.tpz`
- Permessi: `ETC2` e `ASTC` texture compression abilitati

---

## Impostazioni chiave

| Parametro | Valore | Descrizione |
|---|---|---|
| StartSpeed | 10 | Velocità iniziale |
| MaxSpeed | 40 | Velocità massima |
| SpeedIncreaseRate | 1 | Incremento velocità/sec |
| LaneWidth | 2.5 | Larghezza corsia |
| ChunkLength | 40 | Lunghezza sezione tunnel |
| ChunksAhead | 10 | Chunk pre-generati |
| TiltThreshold | 1.5 | Soglia accelerometro |
| TiltSensitivity | 5 | Sensibilità accelerometro |

---

## Cose da fare

- [ ] Aumentare `TiltSensitivity` e `KeyboardSpeed` con il punteggio
- [ ] Effetti sonori per collisione e game over
- [ ] Animazioni schermata game over
- [ ] Pubblicazione Play Store
- [ ] Versione iOS (richiede Mac + Xcode)
