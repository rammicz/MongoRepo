using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoTypeRepository.Wrapper
{
    public class WrappingHelper
    {
        public static IEnumerable<TDb> Wrap<T, TDb>(IEnumerable<T> objectsToWrap) where TDb : AnyTypeWrapper<T> => objectsToWrap.Select(item => (TDb)Activator.CreateInstance(typeof(TDb), item));
    }
}
