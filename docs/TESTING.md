# Manual validation

These are proposed checks, not a claim that every case has passed. The author has reported successful Quest Pro deployment and operation. Record the commit, Unity version, headset/software version, date, and observed failures for a new test run.

## Editor and headset checklist

- Open `Assets/NumberBoardSceneVR.unity`; confirm compilation completes without errors.
- Check all four grid sizes and each of the four rule combinations (16 configurations).
- Verify every number appears once and the next correct target is accepted.
- Verify wrong selections increase the error counter and flash feedback; completed buttons cannot be selected again.
- With ghost enabled, verify odd player turns alternate with even ghost turns. Include both odd-total boards (9 and 25) and even-total boards (16 and 36).
- Verify color-only mode still requires the player to select every number.
- Return to the menu during a ghost delay; ensure no old ghost action affects the next task.
- Restart after completion; confirm a new board is generated and metrics reset.
- Navigate first, middle, and final results pages and check label/log overlap.
- Note the documented total-time-on-pagination issue; do not interpret page changes as extra task execution time.
- On the headset, verify right-trigger selection, ray feedback, placement after recentering, focus recovery, and application exit.
- Check readability on the actual headset, especially the 6 x 6 board and the results panel.

No automated test suite is supplied for the task scripts. Editor mouse checks do not substitute for headset validation.
