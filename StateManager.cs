using System;
using System.Collections.Generic;

public class StateManager
{
    public class StateChangedEventArgs : EventArgs
    {
        public string newState;
        public string previousState;
        public StateChangedEventArgs(string _newState, string _previousState)
        {
            newState = _newState;
            previousState = _previousState;
        }
    }

    public class StateChangedFromEventArgs : EventArgs
    {
        public string newState;
        public StateChangedFromEventArgs(string _newState)
        {
            newState = _newState;
        }
    }

    public class StateChangedToEventArgs : EventArgs
    {
        public string previousState;
        public StateChangedToEventArgs(string _previousState)
        {
            previousState = _previousState;
        }
    }

    class StateEvents
    {
        public EventHandler<StateChangedToEventArgs> changedTo;
        public EventHandler<StateChangedFromEventArgs> changedFrom;
        public StateEvents(EventHandler<StateChangedToEventArgs> _changedTo, EventHandler<StateChangedFromEventArgs> _changedFrom)
        {
            changedTo = _changedTo;
            changedFrom = _changedFrom;
        }
    }

    string _currentState = "Default";
    public string currentState { get { return _currentState; } set { SetState(value); } }

    Dictionary<string, StateEvents> states = new Dictionary<string, StateEvents>()
    {
        {"Default", new StateEvents(
            new EventHandler<StateChangedToEventArgs>(delegate (object sender, StateChangedToEventArgs args) { }),
            new EventHandler<StateChangedFromEventArgs>(delegate (object sender, StateChangedFromEventArgs args) { })
        )}
    };

    EventHandler<StateChangedEventArgs> stateChanged = new EventHandler<StateChangedEventArgs>(delegate (object sender, StateChangedEventArgs args) { });

    /// <summary>
    /// Binds the given handler to the "State Changed" event
    /// </summary>
    public void Bind(EventHandler<StateChangedEventArgs> handler)
    {
        stateChanged += handler;
    }

    /// <summary>
    /// Binds the given handler to the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Bind(string state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!states.ContainsKey(state))
        {
            return false;
        }

        states[state].changedFrom += handler;
        return true;
    }

    /// <summary>
    /// Binds the given handler to the "State Changed To" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Bind(string state, EventHandler<StateChangedToEventArgs> handler)
    {
        if (!states.ContainsKey(state))
        {
            return false;
        }

        states[state].changedTo += handler;
        return true;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed" event
    /// </summary>
    public void Unbind(EventHandler<StateChangedEventArgs> handler)
    {
        stateChanged -= handler;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Unbind(string state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!states.ContainsKey(state))
        {
            return false;
        }

        states[state].changedFrom -= handler;
        return true;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed To" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Unbind(string state, EventHandler<StateChangedToEventArgs> handler)
    {
        if (!states.ContainsKey(state))
        {
            return false;
        }

        states[state].changedTo -= handler;
        return true;
    }

    /// <returns>
    /// false if the given state does not exits, otherwise true
    /// </returns>
    public bool SetState(string state)
    {
        if (!states.ContainsKey(state))
        {
            return false;
        }

        string previousState = _currentState;
        _currentState = state;
        stateChanged.Invoke(this, new StateChangedEventArgs(state, previousState));
        states[previousState].changedFrom.Invoke(this, new StateChangedFromEventArgs(state));
        states[state].changedTo.Invoke(this, new StateChangedToEventArgs(previousState));

        return true;
    }

    /// <returns>
    /// false if the given state already exits, otherwise true
    /// </returns>
    public bool AddState(string state)
    {
        if (states.ContainsKey(state))
        {
            return false;
        }

        states.Add(state, new StateEvents(
            new EventHandler<StateChangedToEventArgs>(delegate { }),
            new EventHandler<StateChangedFromEventArgs>(delegate { })
        ));

        return true;
    }

    /// <returns>
    /// false if the given state is "Default", otherwise true
    /// </returns>
    public bool RemoveState(string state)
    {
        if (state.Equals("Default"))
        {
            return false;
        }

        return states.Remove(state);
    }

    public HashSet<string> GetStates()
    {
        return new HashSet<string>(states.Keys);
    }

    public int GetStateCount()
    {
        return states.Count;
    }

    public bool HasState(string state)
    {
        return states.ContainsKey(state);
    }
}
