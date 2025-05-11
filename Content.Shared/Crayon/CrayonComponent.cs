using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Crayon
{
<<<<<<<< HEAD:Content.Shared/Crayon/SharedCrayonComponent.cs
    [NetworkedComponent, ComponentProtoName("Crayon"), Access(typeof(SharedCrayonSystem))]
    public abstract partial class SharedCrayonComponent : Component
    {
        public string SelectedState { get; set; } = string.Empty;

        [DataField("color")] public Color Color;
========

    /// <summary>
    /// Component holding the state of a crayon-like component
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class CrayonComponent : Component
    {
        /// <summary>
        /// Color with which the crayon will draw
        /// </summary>
        [DataField, AutoNetworkedField]
        public Color Color;
>>>>>>>> c8e9e5fa36 (review comments; also added rotation to item status panel):Content.Shared/Crayon/CrayonComponent.cs

        /// <summary>
        /// The ID of currently selected decal prototype that will be placed when the crayon is used
        /// </summary>
        [ViewVariables, AutoNetworkedField]
        public string State;

        /// <summary>
        /// The amount of times the crayon can used to draw.
        /// </summary>
        [DataField, AutoNetworkedField]
        public int Charges = 30;

        /// <summary>
        /// The total starting charges for the crayon.
        /// </summary>
        [DataField, AutoNetworkedField]
        public int Capacity = 30;

        /// <summary>
        /// The stored rotation for this crayon's decal, in degrees.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Rotation;

        /// <summary>
        /// The active/unactive status of the preview mode overlay for this crayon.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool PreviewMode;

        /// <summary>
        /// The sound to play when drawing with the crayon.
        /// </summary>
        [DataField]
        public SoundSpecifier? UseSound;

        /// <summary>
        /// Can this crayon select its color? e.g. rainbow crayon
        /// </summary>
        [DataField]
        public bool SelectableColor { get; set; }

        /// <summary>
        /// Does this crayon get deleted when the charges are used up?
        /// </summary>
        [DataField]
        public bool DeleteEmpty = true;

        [ViewVariables]
        public bool UIUpdateNeeded;

        [Serializable, NetSerializable]
        public enum CrayonUiKey : byte
        {
            Key,
        }
    }

    [Serializable, NetSerializable]
    public sealed class CrayonSelectMessage : BoundUserInterfaceMessage
    {
        public readonly string State;
        public CrayonSelectMessage(string selected)
        {
            State = selected;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CrayonColorMessage : BoundUserInterfaceMessage
    {
        public readonly Color Color;
        public CrayonColorMessage(Color color)
        {
            Color = color;
        }
    }

<<<<<<<< HEAD:Content.Shared/Crayon/SharedCrayonComponent.cs
========
    /// <summary>
    /// Server to CLIENT. Notifies the BUI that a decal with given ID has been drawn.
    /// Allows the client UI to advance forward in the client-only ephemeral queue,
    /// preventing the crayon from becoming a magic text storage device.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class CrayonUsedMessage : BoundUserInterfaceMessage
    {
        public readonly string DrawnDecal;

        public CrayonUsedMessage(string drawn)
        {
            DrawnDecal = drawn;
        }
    }

    /// <summary>
    /// Client to server message. The user has updated the rotation in the UI,
    /// and the component needs to be updated.
    /// </summary>
>>>>>>>> c8e9e5fa36 (review comments; also added rotation to item status panel):Content.Shared/Crayon/CrayonComponent.cs
    [Serializable, NetSerializable]
    public sealed class CrayonRotationMessage(float rotation) : BoundUserInterfaceMessage
    {
        public float Rotation { get; } = rotation;
    }


    /// <summary>
    /// Client to server message. The user has toggled the preview mode
    /// in the UI, and the component needs to be updated.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class CrayonPreviewModeMessage(bool previewMode) : BoundUserInterfaceMessage
    {
        public bool PreviewMode { get; } = previewMode;
    }

    [Serializable, NetSerializable]
    public enum CrayonVisuals
    {
        State,
        Color
    }

<<<<<<<< HEAD:Content.Shared/Crayon/SharedCrayonComponent.cs
    [Serializable, NetSerializable]
    public sealed class CrayonComponentState : ComponentState
    {
        public readonly Color Color;
        public readonly string State;
        public readonly int Charges;
        public readonly int Capacity;
        public readonly float Rotation;
        public readonly bool PreviewMode;

        public CrayonComponentState(Color color, string state, int charges, int capacity, float rotation, bool previewMode)
        {
            Color = color;
            State = state;
            Charges = charges;
            Capacity = capacity;
            Rotation = rotation;
            PreviewMode = previewMode;
        }
    }
========
    /// <summary>
    /// The state of the crayon UI as sent by the server
    /// </summary>
>>>>>>>> c8e9e5fa36 (review comments; also added rotation to item status panel):Content.Shared/Crayon/CrayonComponent.cs
    [Serializable, NetSerializable]
    public sealed class CrayonBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string Selected;
        public bool SelectableColor;
        public Color Color;
        public float Rotation;
        public bool PreviewMode;

        public CrayonBoundUserInterfaceState(string selected, bool selectableColor, Color color, float rotation, bool previewMode)
        {
            Selected = selected;
            SelectableColor = selectableColor;
            Color = color;
            Rotation = rotation;
            PreviewMode = previewMode;
        }
    }
}
