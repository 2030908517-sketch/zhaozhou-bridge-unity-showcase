# Original environment

- Unity editor: 2021.3.45f2c1, as recorded in ProjectVersion.txt.
- Unity UI and TextMesh Pro used by custom scripts.
- The original package manifest included TextMesh Pro 3.0.6, Universal Render Pipeline 12.1.15 and Input System 1.7.0.
- The scripts themselves use legacy Input.GetKey / Input.GetMouseButtonDown / Input.GetAxis.

An imported project's Active Input Handling setting must allow the legacy Input API (Input Manager or Both). Merely installing the newer Input System package does not convert the scripts to its API.

The original package manifest and global project settings are not distributed as a runnable project here. The showcase has no scene or build configuration.
