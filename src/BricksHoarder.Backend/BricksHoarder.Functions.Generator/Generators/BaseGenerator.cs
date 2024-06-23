namespace BricksHoarder.Functions.Generator.Generators
{
    internal abstract class BaseGenerator
    {
        protected bool IsSaga(Type type)
        {
            return type.Name.EndsWith("Saga");
        }

        protected bool IsUsedBySaga(Type saga, Type @event)
        {
            var properties = saga.GetProperties();
            return properties.Any(p => IsInSaga(p.PropertyType, @event));
        }

        private bool IsInSaga(Type property, Type @event)
        {
            var @args = property.GetGenericArguments();
            var argsEvent = @event.GetGenericArguments();

            if (@args.Length != argsEvent.Length)
            {
                return false;
            }

            for (int i = 0; i < @args.Length; i++)
            {
                if (@args[i].Name != argsEvent[i].Name)
                {
                    return false;
                }

                if (!IsInSaga(args[i], argsEvent[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckGeneric(Type[] types, Type @event)
        {
            foreach (var type in types)
            {
                if (type.IsGenericType)
                {
                    return CheckGeneric(type.GetGenericArguments(), @event);
                }

                return type == @event;
            }

            return false;
        }
    }
}