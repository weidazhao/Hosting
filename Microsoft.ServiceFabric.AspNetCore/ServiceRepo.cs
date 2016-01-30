using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class ServiceRepo
    {
        private readonly object _lockObject = new object();
        private readonly Dictionary<Guid, object> _serviceRepo = new Dictionary<Guid, object>();

        public static readonly ServiceRepo Instance = new ServiceRepo();

        private ServiceRepo()
        {
        }

        public void AddService(Guid id, object service)
        {
            lock (_lockObject)
            {
                _serviceRepo.Add(id, service);
            }
        }

        public void RemoveService(Guid id)
        {
            lock (_lockObject)
            {
                _serviceRepo.Remove(id);
            }
        }

        public KeyValuePair<Guid, object> GetService(HttpRequest request)
        {
            var pathSegments = request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 1)
            {
                return default(KeyValuePair<Guid, object>);
            }

            Guid id;
            if (!Guid.TryParse(pathSegments[0], out id))
            {
                return default(KeyValuePair<Guid, object>);
            }

            lock (_lockObject)
            {
                object service;
                if (!_serviceRepo.TryGetValue(id, out service))
                {
                    return default(KeyValuePair<Guid, object>);
                }

                return new KeyValuePair<Guid, object>(id, service);
            }
        }
    }
}