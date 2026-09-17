# Meta Quest VR Cognitive Task

A Unity application for a randomized Schulte-style number-selection task, with configurable ghost turns and odd/even color cues. Built with C#, Meta XR SDK, and OpenXR.

The task uses headset pose and controller input; it does not require eye tracking or other Quest Pro-specific features. Other Meta Quest models have not yet been verified for this project's current build configuration.

**Status:** Android APK deployment and on-device operation on Meta Quest Pro have been reported as successful by the project author. This repository preparation did not include a new build or headset test. The application is a development prototype; its timing and cognitive outcomes have not been scientifically validated.

## Features

- Four grid sizes: 3 x 3, 4 x 4, 5 x 5, and 6 x 6.
- Randomized positions for numbers 1 through the selected grid's size squared.
- Independent ghost and color-cue switches, providing four rule configurations per size.
- Right-controller ray selection and mouse input for Editor debugging.
- Correct/incorrect visual feedback and ordered-selection validation.
- In-memory timing and accuracy summaries, with up to six log entries per page.
- Restart, return-to-menu, and application-exit actions.
- Task placement in front of the headset at startup, with repositioning after tracking-origin changes or focus recovery.

## Technology

| Component | Project version / usage |
| --- | --- |
| Unity | 6000.3.16f1 (Unity 6.3 LTS) |
| Language | C# |
| Meta XR SDK All | 203.0.0 |
| OpenXR Plugin | 1.17.1 |
| XR Interaction Toolkit | 3.3.2 is a package dependency; the task's core selection uses custom physics raycasts |
| Input | Unity XR device input, OVRInput, and Unity Input System for mouse input |
| Text | TextMesh for buttons and TextMeshPro for results |

Exact dependencies are recorded in [manifest.json](Packages/manifest.json) and [packages-lock.json](Packages/packages-lock.json). There is no React frontend, Node.js backend, or database.

## Open and run

1. Install Unity **6000.3.16f1** using Unity Hub, including Android Build Support, SDK & NDK Tools, and OpenJDK for device builds.
2. Clone or download this repository and add the root folder containing `Assets`, `Packages`, and `ProjectSettings` to Unity Hub.
3. Open the project and allow package restoration and asset import to finish.
4. Open **`Assets/NumberBoardSceneVR.unity`**. Other scenes are historical experiments and are not the configured build entry point.
5. Enter Play Mode for a mouse-based smoke test. Click buttons in the Game view. Missing-controller warnings can occur without a connected headset.

For Meta Quest devices, select Android in Build Profiles and confirm that the scene list contains `Assets/NumberBoardSceneVR.unity` enabled. If a profile overrides the scene list, verify that list as well. Use a headset configured for development and authorize the USB connection before building and installing an APK. Keep generated builds under `Builds/`, which is ignored by Git.

## How to play

1. Choose a grid size from the main menu.
2. Configure the rules, then select **START**. There is no separate Skip button.
3. Select the numbers in ascending order using the right controller's index trigger, or the mouse in the Editor.
4. On completion, review the summary and page through the records. **RESTART** generates a new randomized board with the same size and rules; **RETURN TO MENU** returns to size selection.

| Ghost | Colors | Behavior |
| --- | --- | --- |
| Off | Off | Player selects all numbers in order. |
| Off | On | Player selects all numbers; odd numbers are blue and even numbers orange. |
| On | Off | Player selects odd numbers; the ghost automatically completes even numbers after a configured delay. |
| On | On | Same alternating turns, with blue player numbers and orange ghost numbers. |

Selecting an incorrect number or selecting during the ghost's turn counts as a mistake. Completed number buttons are disabled. **RETURN** during a task abandons that task and returns to the menu. Rule choices persist within the running application until changed. **EXIT APP** exits the application; in the Editor it stops Play Mode.

## Implementation

| Script | Responsibility |
| --- | --- |
| [NumberBoardTask.cs](Assets/Scripts/NumberBoardTask.cs) | Grid generation, menu flow, rule configuration, validation, ghost coroutine, timing, and results |
| [NumberButton.cs](Assets/Scripts/NumberButton.cs) | Number label, selection forwarding, visual feedback, and collider disabling |
| [BoardActionButton.cs](Assets/Scripts/BoardActionButton.cs) | Runtime menu buttons and action dispatch |
| [QuestControllerSelector.cs](Assets/Scripts/QuestControllerSelector.cs) | Right-controller input, physics raycasting, visible ray, and selectable dispatch |
| [MouseClickSelector.cs](Assets/Scripts/MouseClickSelector.cs) | Mouse raycasting into the same number/menu handlers |
| [CenterTaskInView.cs](Assets/Scripts/CenterTaskInView.cs) | Camera-relative placement and tracking-origin event handling |

Controller and mouse selectors call `NumberButton.Select()` or `BoardActionButton.Select()`. Number selection reaches `NumberBoardTask.SelectNumber()`, which validates the current turn and target. Number buttons are instantiated from `Assets/Prefabs/NumberButtonPrefab.prefab` under `TaskRoot`; menu buttons are generated at runtime.

The manager's Inspector references are `numberButtonPrefab`, `taskRoot`, `resultPanel`, and `resultText`. The controller selector additionally requires `rayOrigin`.

## Metrics and limitations

- Player accuracy is `correctClicks / (correctClicks + wrongClicks)`. Ghost actions are excluded from this calculation.
- A successful player selection's interval is measured from task start or the previous completed number. In ghost mode, the next player interval starts after the ghost finishes. Incorrect attempts do not reset this interval.
- Average reaction time is the mean of successful player intervals. Timing uses Unity `Time.time`; displayed decimal precision does not imply calibrated measurement accuracy.
- Standard/color-only logs contain successful selection intervals; incorrect selections contribute to the error counter. Ghost-mode logs also contain player mistakes and automated actions (`P` = player, `G` = ghost, `OK`/`MISS` = outcome).
- **Known issue:** `Total Time` is recalculated when a results page is rendered, so changing pages after completion increases the displayed value. It is not yet a frozen completion timestamp.
- Records are held in memory and reset on a new task. CSV/JSON export, participant IDs, persistent storage, and a statistical analysis pipeline are not implemented.
- Legacy 1-to-10 Matching Numbers methods remain in the source but are not exposed in the current menu.
- Eye tracking, automated regression coverage, and performance benchmarks are not included. UI layouts use fixed local dimensions and should be checked on the intended headset and display aspect ratio.

See [manual checks](docs/TESTING.md) for a repeatable validation checklist and [publishing notes](docs/PUBLISHING.md) for the first GitHub upload.

## Repository and licensing

Keep `Assets` (including `.meta` files), `Packages`, and `ProjectSettings` together. Unity regenerates the ignored `Library` directory on import.

No project-wide open-source license has been selected. Publishing the repository does not grant a blanket reuse license for its contents. Unity, Meta, fonts, and other third-party materials retain their own terms and notices.
