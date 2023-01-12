using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// A class for managing state. It will always contain the "Default" state.
/// </summary>
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

    public class StateEvents
    {
        public EventHandler<StateChangedToEventArgs> changedTo;
        public EventHandler<StateChangedFromEventArgs> changedFrom;
        public StateEvents(EventHandler<StateChangedToEventArgs> _changedTo, EventHandler<StateChangedFromEventArgs> _changedFrom)
        {
            changedTo = _changedTo;
            changedFrom = _changedFrom;
        }
    }

    const string _defaultState = "Default";
    public static readonly string defaultState = _defaultState;

    string _currentState = _defaultState;
    public string currentState { get { return _currentState; } set { SetState(value); } }

    Dictionary<string, StateEvents> _states = new Dictionary<string, StateEvents>()
    {
        {_defaultState, new StateEvents(
            new EventHandler<StateChangedToEventArgs>(delegate (object sender, StateChangedToEventArgs args) { }),
            new EventHandler<StateChangedFromEventArgs>(delegate (object sender, StateChangedFromEventArgs args) { })
        )}
    };

    EventHandler<StateChangedEventArgs> _stateChanged = new EventHandler<StateChangedEventArgs>(delegate (object sender, StateChangedEventArgs args) { });

    public StateManager() { }

    public StateManager(IEnumerable<string> states, string initialState = _defaultState)
    {
        foreach (string state in states)
        {
            AddState(state);
        }

        if (_states.ContainsKey(initialState))
        {
            _currentState = initialState;
        }
    }

    public StateManager(IEnumerable<KeyValuePair<string, StateEvents>> states, string initialState = _defaultState) : this(from state in states select state.Key, initialState)
    {
        foreach (KeyValuePair<string, StateEvents> state in states)
        {
            Bind(state.Key, state.Value.changedFrom);
            Bind(state.Key, state.Value.changedTo);
        }
    }

    public StateManager(EventHandler<StateChangedEventArgs> handler, IEnumerable<KeyValuePair<string, StateEvents>> states = null, string initialState = _defaultState) : this(states ?? Enumerable.Empty<KeyValuePair<string, StateEvents>>(), initialState)
    {
        Bind(handler);
    }

    /// <summary>
    /// Binds the given handler to the "State Changed" event
    /// </summary>
    public void Bind(EventHandler<StateChangedEventArgs> handler)
    {
        _stateChanged += handler;
    }

    /// <summary>
    /// Binds the given handler to the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Bind(string state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedFrom += handler;
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
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedTo += handler;
        return true;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed" event
    /// </summary>
    public void Unbind(EventHandler<StateChangedEventArgs> handler)
    {
        _stateChanged -= handler;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Unbind(string state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedFrom -= handler;
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
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedTo -= handler;
        return true;
    }

    /// <returns>
    /// false if the given state does not exits, otherwise true
    /// </returns>
    public bool SetState(string state)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        string previousState = _currentState;
        _currentState = state;
        _stateChanged.Invoke(this, new StateChangedEventArgs(state, previousState));
        _states[previousState].changedFrom.Invoke(this, new StateChangedFromEventArgs(state));
        _states[state].changedTo.Invoke(this, new StateChangedToEventArgs(previousState));

        return true;
    }

    /// <returns>
    /// false if the given state already exits, otherwise true
    /// </returns>
    public bool AddState(string state)
    {
        if (_states.ContainsKey(state))
        {
            return false;
        }

        _states.Add(state, new StateEvents(
            new EventHandler<StateChangedToEventArgs>(delegate { }),
            new EventHandler<StateChangedFromEventArgs>(delegate { })
        ));

        return true;
    }

    /// <returns>
    /// false if the given state is the default state, otherwise true
    /// </returns>
    public bool RemoveState(string state)
    {
        if (state.Equals(_defaultState))
        {
            return false;
        }

        return _states.Remove(state);
    }

    public HashSet<string> GetStates()
    {
        return new HashSet<string>(_states.Keys);
    }

    public int GetStateCount()
    {
        return _states.Count;
    }

    public bool HasState(string state)
    {
        return _states.ContainsKey(state);
    }
}
