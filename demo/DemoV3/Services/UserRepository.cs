using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DemoV3.Errors;
using DemoV3.Model;

namespace DemoV3.Services
{
    public class UserRepository<T> where T : AnyUser
    {
        private readonly ConcurrentDictionary<string, T> _fakeRepo = new ConcurrentDictionary<string, T>();

        public async Task<ICollection<T>> List()
        {
            await Task.Yield();
            return _fakeRepo.Values;
        }

        public async Task<T> GetById(string id)
        {
            await Task.Yield();
            if (_fakeRepo.TryGetValue(id, out var entity))
                return entity;
            throw new UserNotFoundClientError();
        }

        public async Task Add(string id, T entity)
        {
            await Task.Yield();
            _fakeRepo[id] = entity;
        }

        public async Task Delete(string id)
        {
            await Task.Yield();
            _fakeRepo.TryRemove(id, out _);
        }

        public async Task Edit(string id, T entity)
        {
            await Task.Yield();
            if (!_fakeRepo.TryGetValue(id, out _))
                throw new UserNotFoundClientError();

            _fakeRepo[id] = entity;
        }
    }
}