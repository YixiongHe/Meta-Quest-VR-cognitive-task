# Publish to GitHub

The Unity project root is the folder containing `Assets`, `Packages`, `ProjectSettings`, and `README.md`. Do not upload the parent directory or the local `NumberBoardScene Project` folder.

## First commit

The following commands are instructions for the owner; repository initialization and publication were not performed when preparing these documents.

Run from the project root:

```sh
git init -b main
git add README.md .gitignore .gitattributes docs Assets Packages ProjectSettings
git status --short
git diff --cached --stat
```

Inspect the staged list. It should contain Unity source assets and their `.meta` files, package manifests/lockfile, settings, and documentation. Generated caches, recovery scenes, APKs, and signing keys should be absent. Review any personal information or proprietary assets before making the repository public.

```sh
git commit -m "Document and publish VR cognitive task prototype"
```

Create an empty GitHub repository (suggested name: `quest-vr-cognitive-task`). Do not initialize another README there. Replace the placeholder URL below with your repository URL:

```sh
git remote add origin https://github.com/YOUR-USERNAME/quest-vr-cognitive-task.git
git push -u origin main
```

Git may prompt for your configured GitHub credentials. Never put access tokens in source files or remote URLs.

## Repository presentation

Suggested description:

> Unity/C# Schulte-style VR task for Meta Quest with configurable ghost turns, color cues, controller selection, and in-app timing results.

Suggested topics: `unity`, `csharp`, `virtual-reality`, `meta-quest`, `openxr`, `schulte-table`.

An optional headset recording and screenshots from the current build would help visitors understand the app. Add only media you have reviewed and link it from the README. Publish distributable APKs as release attachments rather than source commits, subject to dependency redistribution terms.

## Scope of the preparation check

- No Git repository existed at the project root during preparation.
- The largest file found in `Assets`, `Packages`, and `ProjectSettings` was approximately 2.3 MB. Git LFS was not configured for the current files.
- Android signing key/alias paths were empty in the inspected project settings. A limited scan found no matching private-key or common token patterns in the inspected source/settings; this is not an exhaustive secret or license audit.
- Recovery scenes and generated/local directories are excluded by `.gitignore`; they remain on disk.
- No project-wide license has been chosen. Select one for code you own after reviewing third-party terms if you intend to permit open-source reuse.

## Clean-clone validation

After uploading, clone into a separate directory, open with the recorded Unity version, restore packages, and run the [manual checklist](TESTING.md). This verifies that no local-only files are required. The existing successful headset test does not establish clean-clone reproducibility.
