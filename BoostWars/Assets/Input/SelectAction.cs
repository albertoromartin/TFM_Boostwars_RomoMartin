//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Input/SelectAction.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @SelectAction : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @SelectAction()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""SelectAction"",
    ""maps"": [
        {
            ""name"": ""MapSelector"",
            ""id"": ""dd3be88f-ba11-4220-b874-c16296af4b93"",
            ""actions"": [
                {
                    ""name"": ""PrimaryPosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""09d725bf-5804-4a67-a8ba-873549c560bb"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Press"",
                    ""type"": ""Button"",
                    ""id"": ""cf38b44a-eaf7-4c76-84ff-c0e4baeabbc9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""872abf0e-592d-49c0-9ed3-31d6a2ba549c"",
                    ""path"": ""<Touchscreen>/primaryTouch/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d361f058-2208-471e-a037-c22735ba423b"",
                    ""path"": ""<Touchscreen>/primaryTouch/press"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Press"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MapSelector
        m_MapSelector = asset.FindActionMap("MapSelector", throwIfNotFound: true);
        m_MapSelector_PrimaryPosition = m_MapSelector.FindAction("PrimaryPosition", throwIfNotFound: true);
        m_MapSelector_Press = m_MapSelector.FindAction("Press", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // MapSelector
    private readonly InputActionMap m_MapSelector;
    private IMapSelectorActions m_MapSelectorActionsCallbackInterface;
    private readonly InputAction m_MapSelector_PrimaryPosition;
    private readonly InputAction m_MapSelector_Press;
    public struct MapSelectorActions
    {
        private @SelectAction m_Wrapper;
        public MapSelectorActions(@SelectAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @PrimaryPosition => m_Wrapper.m_MapSelector_PrimaryPosition;
        public InputAction @Press => m_Wrapper.m_MapSelector_Press;
        public InputActionMap Get() { return m_Wrapper.m_MapSelector; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MapSelectorActions set) { return set.Get(); }
        public void SetCallbacks(IMapSelectorActions instance)
        {
            if (m_Wrapper.m_MapSelectorActionsCallbackInterface != null)
            {
                @PrimaryPosition.started -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPrimaryPosition;
                @PrimaryPosition.performed -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPrimaryPosition;
                @PrimaryPosition.canceled -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPrimaryPosition;
                @Press.started -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPress;
                @Press.performed -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPress;
                @Press.canceled -= m_Wrapper.m_MapSelectorActionsCallbackInterface.OnPress;
            }
            m_Wrapper.m_MapSelectorActionsCallbackInterface = instance;
            if (instance != null)
            {
                @PrimaryPosition.started += instance.OnPrimaryPosition;
                @PrimaryPosition.performed += instance.OnPrimaryPosition;
                @PrimaryPosition.canceled += instance.OnPrimaryPosition;
                @Press.started += instance.OnPress;
                @Press.performed += instance.OnPress;
                @Press.canceled += instance.OnPress;
            }
        }
    }
    public MapSelectorActions @MapSelector => new MapSelectorActions(this);
    public interface IMapSelectorActions
    {
        void OnPrimaryPosition(InputAction.CallbackContext context);
        void OnPress(InputAction.CallbackContext context);
    }
}
