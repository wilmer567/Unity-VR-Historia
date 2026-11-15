# VR Historia — Guia de Escalabilidad del Modo Historia

## Resumen

Este documento define la arquitectura escalable para el **Modo Historia VR** del proyecto Unity Essentials. Cubre tres pilares fundamentales: interacciones con objetos, dialogos inmersivos y teletransporte.

---

## 1. Interacciones con Objetos

### 1.1 Arquitectura Base

| Capa | Responsabilidad | Componente Unity |
|------|----------------|------------------|
| Deteccion | Identificar objetos interactuables en rango | `XRRayInteractor` / `XRDirectInteractor` |
| Feedback | Resaltar objeto al apuntar o acercarse | `XRInteractableVisualController` + shaders de contorno |
| Accion | Agarrar, soltar, usar, inspeccionar | `XRGrabInteractable` + eventos custom |

### 1.2 Tipos de Interaccion

- **Grab (Agarrar)**: Objetos fisicos que el jugador puede tomar y manipular (llaves, herramientas, armas).
- **Inspect (Inspeccionar)**: Objetos que flotan frente al jugador para examinarlos de cerca (documentos, artefactos).
- **Use (Usar)**: Accion contextual sobre un objeto (abrir puerta, activar mecanismo, combinar items).
- **Trigger (Detonar)**: Objetos que activan eventos narrativos al ser tocados o recogidos.

### 1.3 Escalabilidad

```
InteractionProfile (ScriptableObject)
  ├── interactionType: enum { Grab, Inspect, Use, Trigger }
  ├── hapticFeedback: HapticPreset
  ├── audioClip: AudioClip
  ├── requiredItem: ItemID (nullable)
  └── onInteract: UnityEvent
```

Cada objeto interactuable referencia un `InteractionProfile`. Esto permite:
- Agregar nuevos tipos de interaccion sin modificar MonoBehaviours existentes.
- Configurar comportamiento desde el Inspector sin codigo.
- Reutilizar perfiles entre escenas y capitulos.

---

## 2. Dialogos Inmersivos

### 2.1 Sistema de Dialogos

El sistema se basa en un grafo de nodos de dialogo que soporta ramificacion narrativa.

```
DialogueGraph (ScriptableObject)
  ├── nodes: List<DialogueNode>
  │     ├── speakerID: string
  │     ├── text: string
  │     ├── audioClip: AudioClip (voz)
  │     ├── emotion: enum { Neutral, Happy, Angry, Sad, Surprised }
  │     ├── choices: List<DialogueChoice>
  │     │     ├── label: string
  │     │     ├── nextNodeID: string
  │     │     └── condition: GameFlag (nullable)
  │     └── events: List<UnityEvent>
  └── variables: Dictionary<string, object>
```

### 2.2 Presentacion en VR

| Elemento | Implementacion |
|----------|---------------|
| Subtitulos | Panel Canvas World-Space anclado al NPC o flotante |
| Seleccion de respuesta | Raycast desde controlador hacia opciones 3D |
| Expresiones faciales | Blend shapes del NPC sincronizados con `emotion` |
| Audio espacial | `AudioSource` 3D en el NPC con atenuacion por distancia |

### 2.3 Escalabilidad

- **Localizacion**: Los textos se almacenan con clave i18n; el `DialogueGraph` referencia claves, no strings literales.
- **Capitulos**: Cada capitulo del modo historia tiene su propio conjunto de `DialogueGraph` assets, cargados bajo demanda.
- **Condiciones dinamicas**: Las opciones de dialogo se filtran por `GameFlags` (items obtenidos, decisiones previas, progreso).
- **Nuevo contenido**: Agregar dialogos es crear un asset — no requiere cambios en codigo.

---

## 3. Teletransporte

### 3.1 Componentes Base

| Componente | Funcion |
|------------|---------|
| `TeleportationProvider` | Procesa solicitudes de teletransporte en el XR Rig |
| `TeleportationArea` | Superficie valida para teletransportarse (suelo) |
| `TeleportationAnchor` | Punto fijo de teletransporte con orientacion predefinida |
| `XRRayInteractor` (modo teleport) | Arco visual que muestra destino al apuntar |

### 3.2 Modos de Movimiento

1. **Teleport Arc**: El jugador apunta con el controlador y confirma con trigger. Ideal para reducir motion sickness.
2. **Snap Turn**: Rotacion discreta (30/45/90 grados) con el joystick. Complementa el teletransporte.
3. **Continuous Move** (opcional): Movimiento continuo con joystick para jugadores experimentados.

### 3.3 Zonas y Restricciones

```
TeleportZone (ScriptableObject)
  ├── zoneID: string
  ├── displayName: string
  ├── isUnlocked: bool
  ├── requiredFlag: GameFlag (nullable)
  ├── arrivalAnchor: Transform reference
  └── transitionEffect: enum { Fade, Dissolve, Portal }
```

- Las zonas se desbloquean progresivamente segun avanza la historia.
- Cada zona define un punto de llegada (`arrivalAnchor`) y efecto de transicion.
- El `CustomSceneManager` (singleton existente) gestiona la carga de escenas al teletransportarse entre zonas de diferentes scenes.

### 3.4 Escalabilidad

- **Nuevas zonas**: Crear un `TeleportZone` asset y colocar `TeleportationAnchor` en la escena.
- **Transiciones entre escenas**: Integrar con `CustomSceneManager.LoadScene()` para teletransporte que cruza escenas.
- **Mapa de zonas**: Un ScriptableObject maestro (`WorldMap`) lista todas las `TeleportZone` y sus conexiones para generar un minimapa o menu de viaje rapido.

---

## 4. Integracion entre Sistemas

```
[Jugador apunta objeto] --> InteractionProfile.onInteract
    --> DialogueGraph (si el objeto es un NPC)
    --> TeleportZone.Unlock (si el objeto es una llave/item)
    --> InventorySystem.Add (si es un item recogible)

[Jugador completa dialogo] --> GameFlags.Set("quest_01_done")
    --> TeleportZone("zona_secreta").isUnlocked = true
    --> UI Notification ("Nueva zona desbloqueada")
```

### Flujo de un Capitulo

1. El jugador se teletransporta a la zona del capitulo.
2. Interactua con objetos del entorno (pistas, herramientas).
3. Habla con NPCs mediante el sistema de dialogos.
4. Las decisiones y objetos recolectados actualizan `GameFlags`.
5. Se desbloquean nuevas zonas de teletransporte.
6. El jugador avanza al siguiente capitulo.

---

## 5. Paquetes VR Requeridos

| Paquete | Version | Estado |
|---------|---------|--------|
| OpenXR Plugin (`com.unity.xr.openxr`) | 1.18.0 | Instalado |
| XR Interaction Toolkit (`com.unity.xr.interaction.toolkit`) | 3.6.1 | Instalado |
| Input System (`com.unity.inputsystem`) | 1.20.0 | Instalado (preexistente) |
| XR Core Module (`com.unity.modules.xr`) | 1.0.0 | Instalado (built-in) |

---

## 6. Proximos Pasos

1. **Configurar XR Rig**: Crear el prefab `XR Origin` con controladores, ray interactors y teleport provider.
2. **Crear InteractionProfile base**: ScriptableObject plantilla para los tipos Grab, Inspect, Use y Trigger.
3. **Prototipo de DialogueGraph**: Editor visual de nodos o estructura de datos serializable para el primer dialogo.
4. **Zona de prueba**: Escena VR de testeo con area de teletransporte, un NPC con dialogo y tres objetos interactuables.
5. **Configurar OpenXR**: En Project Settings > XR Plug-in Management, activar OpenXR y agregar perfiles de interaccion (Meta Quest, Valve Index, etc.).
