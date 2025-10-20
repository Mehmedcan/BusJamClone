# 🚌 Bus Jam Clone

A lightweight, mobile-oriented Unity project developed for a technical evaluation case.  
This repository demonstrates structured project setup, clean code organization, and custom editor tooling.

---

## 🧩 Game Setup
- **Engine Version:** Unity `2022.3.19f1` (LTS recommended)  
- **Dependencies:** All plugins and assets used are **Free** and **Open Source**  
- It is recommended to run the project in the Editor using **mobile portrait resolution** and **iOS platform settings**.  
- Run the project starting from `Assets/_Project/Scenes/LauncherScene`

---

## 🛠️ Level Editor
- Access via Unity menu: **`Tools > Level Editor`**  
- The editor is driven by a config asset located at  
  `Data/ScriptableObjects/Resources/GameConfig.scriptableobject`  
  - This file should already be assigned; if not, create one and assign it manually.  
- Each function within the editor includes **inline tooltips and explanations**, e.g., hold **Shift** to change *HumanType*.

---

## ⚙️ Technical Highlights
- **Modular and extensible architecture**, allowing easy feature expansion and maintainability.  
- **Custom in-Editor tools** developed to streamline level creation and configuration workflows.  
- **Reactive programming principles (UniRX)** applied for clean, event-driven game logic and UI updates.
