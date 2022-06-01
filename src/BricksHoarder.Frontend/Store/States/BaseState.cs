using Fluxor;
using Newtonsoft.Json;

namespace KalkulatorKredytuHipotecznego.Store.States
{
    public record BaseState<TState>
    {
        [JsonIgnore]
        public static IDispatcher Dispatcher { get; set; }

        public void OnChange<T>(ref T field, T value) where T : struct
        {
            if (Dispatcher == null)
            {
                field = value;
                return;
            }

            bool hasChanged = !field.Equals(value);

            if (hasChanged)
            {
                field = value;
                StateChanged stateChanged = new StateChanged();
                Dispatcher.Dispatch(stateChanged);
            }
        }

        public void OnChange<T>(ref T field, T value, object action) where T : struct
        {
            if (Dispatcher == null)
            {
                field = value;
                return;
            }

            bool hasChanged = !field.Equals(value);

            if (hasChanged)
            {
                field = value;
                Dispatcher.Dispatch(action);
            }
        }
    }
}