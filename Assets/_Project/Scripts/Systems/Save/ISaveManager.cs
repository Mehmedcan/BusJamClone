using _Project.Scripts.Systems.Base;

namespace _Project.Scripts.Systems.Save
{
    public interface ISaveManager : IBaseManager
    {
        public void Save<T>(string key, T data);
        public T Load<T>(string key);
        public void Delete(string key);
    }
}