---
sidebar_position: 3
---

# UI MVP Structure

DressingTools uses the **Passive View variant** of the **Model-View-Presenter (MVP)** structure in its UI design.

DressingTools<sup>2</sup> is a large project with more than 10,000 lines of code, and UI is also a critical component
of the tool. An architectural pattern is certainly needed to organize the code and enforce unit testing.

## Structure

Presenters only updates the View using the interface provided by the View, but not the actual implementation. This
decouples the code and allows mock unit testing to be done. A simplified overview of DressingTools' MVP structure:

[![DT2 MVP Structure Diagram](/img/diagrams/dt2-mvp-structure.drawio.svg)](/img/diagrams/dt2-mvp-structure.drawio.svg)

## Inheritance

The views mainly inherit two parents: ```EditorViewBase``` is our wrapper to enforce MVP patterns and to use event-based updating.
And the another parent is an interface provided by the view to communicate with the presenter.

[![DT2 MVP Inheritance Diagram](/img/diagrams/dt2-mvp-inheritance.drawio.svg)](/img/diagrams/dt2-mvp-inheritance.drawio.svg)

### Unity IMGUI

Unity IMGUI does not support using the MVP structure since  is so simple that you just have to add UI elements
inside the ```OnGUI()``` loop and it will keep calling it during GUI updates.

```csharp
public void OnGUI() {
    GUILayout.Label("Hello World!");
}
```

### EditorViewBase wrapper

Thus, DressingTools created a wrapper that wraps the IMGUI along with C# event and delegates, enabling to use the MVP
structure that keeps business logic away from the view.

The view script looks cleaner and easier to manage than using plain IMGUI in our project.

```csharp
public override void OnGUI()
{
    if (ShowCreateCabinetWizard)
    {
        Label("There are no existing cabinets. Create one below for your avatar:");
        GameObjectField("Avatar", ref _selectedCreateCabinetGameObject, true);
        Button("Create cabinet", CreateCabinetButtonClick);
    }

    if (ShowCabinetWearables)
    {
        // create dropdown menu for cabinet selection
        Popup("Cabinet", ref _selectedCabinetIndex, AvailableCabinetSelections, SelectedCabinetChange);

        GameObjectField("Avatar", ref _cabinetAvatarGameObject, true, CabinetSettingsChange);
        TextField("Armature Name", ref _cabinetAvatarArmatureName, CabinetSettingsChange);

        var copy = new List<WearablePreview>(WearablePreviews);
        foreach (var preview in copy)
        {
            Label(preview.name);
            Button("Remove", preview.RemoveButtonClick);
        }

        Button("Add Wearable", AddWearableButtonClick);
    }
}
```

### ElementViewBase wrapper

A wrapper of the Unity UI Toolkit / UIElements system. Now most DressingTools GUI uses this `ElementViewBase` along with UXML, USS layout files to design the user interfaces.
