
# Messaging System

The uGUI system uses a messaging system that replaces [`SendMessage`](https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html). It's written entirely in C# and addresses several limitations of `SendMessage`. To receive a callback, implement one of the custom interfaces on a `MonoBehaviour`.

Each call specifies a target GameObject and a messaging interface. Unity issues the call on every component of the target GameObject that implements the interface. A call can include custom user data. It can also control how far the event travels through the GameObject hierarchy: to the target GameObject only, or to the target and then each parent in turn. The framework also provides helper functions that find GameObjects that implement a given messaging interface.

The messaging system is generic, so general game code can use it and not only the UI system. Adding custom messaging events is simple, and they work through the same framework that the UI system uses for all event handling.

## Defining A Custom Message

To define a custom message, start from the base interface [`IEventSystemHandler`](xref:UnityEngine.EventSystems.IEventSystemHandler) in the `UnityEngine.EventSystems` namespace. Anything that extends this interface can act as a target that receives events through the messaging system.

````
public interface ICustomMessageTarget : IEventSystemHandler
{
    // functions that can be called via the messaging system
    void Message1();
    void Message2();
}
````

A MonoBehaviour can then implement this interface. The implementation defines the functions that run when a message targets the GameObject of that MonoBehaviour.

````
public class CustomMessageTarget : MonoBehaviour, ICustomMessageTarget
{
    public void Message1()
    {
        Debug.Log ("Message 1 received");
    }

    public void Message2()
    {
        Debug.Log ("Message 2 received");
    }
}
````

With a script in place to receive the message, the next step is to issue it. A message often responds to a decoupled event elsewhere in the application. The uGUI system, for example, issues events such as `PointerEnter` and `PointerExit`, listed in [Supported Events](SupportedEvents.md), along with other events that respond to user input.

A static helper class sends the message. It takes a target object, user-specific data, and a functor that maps to the function you want to call in the message interface.

````
ExecuteEvents.Execute<ICustomMessageTarget>(target, null, (x,y)=>x.Message1());
````

This code will execute the function Message1 on any components on the GameObject target that implement the ICustomMessageTarget interface. The scripting documentation for the [`ExecuteEvents`](xref:UnityEngine.EventSystems.ExecuteEvents) class covers the other form, `ExecuteHierarchy`, which starts at the target GameObject and walks up through its parents.
