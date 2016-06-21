using System.Collections.Generic;

namespace MockEF
{
    public interface IContextCommand<T> where T : class 
    {
        void Insert(List<T> data);
        void Update(List<T> data);
    }
}
