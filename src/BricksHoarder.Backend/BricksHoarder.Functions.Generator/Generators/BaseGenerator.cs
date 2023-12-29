namespace BricksHoarder.Functions.Generator.Generators
{
    internal abstract class BaseGenerator
    {
        protected bool IsSaga(Type type)
        {
            return type.Name.EndsWith("Saga");
        }

        protected bool IsEventUsedBySaga(Type saga, Type @event)
        {
            var properties = saga.GetProperties();
            return properties.Any(p => IsEventInSaga(p.PropertyType, @event));
        }

        private bool IsEventInSaga(Type property, Type @event)
        {
            if (!property.IsGenericType)
            {
                return false;
            }

            var @args = property.GetGenericArguments();
            return CheckGeneric(args, @event);
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