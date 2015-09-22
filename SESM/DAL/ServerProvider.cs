using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceProcess;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.DAL
{
    public class ServerProvider
    {
        private readonly DataContext _context;

        public ServerProvider(DataContext context)
        {
            _context = context;
        }


        // Port Check

        /// <summary>
        /// Check if a server port is in use by another server
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsPortAvailable(int port, EntityServer server)
        {
            try
            {
                EntityServer testServ = _context.Servers.First(serv => serv.Port == port);
                if (testServ == null)
                    return true;

                if (testServ.Id == server.Id)
                    return true;
                return false;


            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public int GetNextAvailablePort(string ip = "")
        {
            if (string.IsNullOrWhiteSpace(ip))
                ip = SEDefault.IP;

            if (!_context.Servers.Any(server => server.Ip == ip))
                return SEDefault.ServerPort;
            return _context.Servers.Where(server => server.Ip == ip).Select(server => server.Port).Max() + 1;
        }
        
        public EntityServer GetServer(int id)
        {
            try
            {
                return _context.Servers.First(s => s.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ServiceState GetState(EntityServer server)
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController service;
            try
            {
                service = services.First(x => x.ServiceName == ServiceHelper.GetServiceName(server));
            }
            catch (Exception)
            {
                service = null;
            }
            if (service == null)
                return ServiceState.Unknow;
            if (service.Status == ServiceControllerStatus.Running)
                return ServiceState.Running;
            if (service.Status == ServiceControllerStatus.StartPending)
                return ServiceState.Starting;
            if (service.Status == ServiceControllerStatus.StopPending)
                return ServiceState.Stopping;
            if (service.Status == ServiceControllerStatus.Stopped)
                return ServiceState.Stopped;
            return ServiceState.Unknow;
        }

        public Dictionary<EntityServer, ServiceState> GetState(List<EntityServer> servers)
        {
            Dictionary<EntityServer, ServiceState> serversStates = new Dictionary<EntityServer, ServiceState>();

            ServiceController[] services = ServiceController.GetServices();

            foreach (EntityServer item in servers)
            {
                ServiceController service;
                try
                {
                    service = services.First(x => x.ServiceName == ServiceHelper.GetServiceName(item));
                }
                catch (Exception)
                {
                    service = null;
                }
                ServiceState state = ServiceState.Unknow;
                if (service == null)
                    state = ServiceState.Unknow;
                else if (service.Status == ServiceControllerStatus.Running)
                    state = ServiceState.Running;
                else if (service.Status == ServiceControllerStatus.StartPending)
                    state = ServiceState.Starting;
                else if (service.Status == ServiceControllerStatus.StopPending)
                    state = ServiceState.Stopping;
                else if (service.Status == ServiceControllerStatus.Stopped)
                    state = ServiceState.Stopped;

                serversStates.Add(item, state);
            }

            return serversStates;
        }

        public bool IsNameAvaialble(string name)
        {
            try
            {
                return !_context.Servers.Any(s => s.Name == name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateServer(EntityServer server)
        {
            try
            {
                _context.Servers.Attach(server);
                _context.Entry<EntityServer>(server).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void CreateServer(EntityServer server)
        {
            try
            {
                _context.Servers.Add(server);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }


        public List<EntityServer> GetAllServers()
        {
            return _context.Servers.ToList();
        }

        public List<EntityServer> GetAllSEServers()
        {
            return _context.Servers.Where(item => item.ServerType == EnumServerType.SpaceEngineers).ToList();
        }

        public List<EntityServer> GetAllMEServers()
        {
            return _context.Servers.Where(item => item.ServerType == EnumServerType.MedievalEngineers).ToList();
        }

        public List<EntityServer> GetAllSESEServers()
        {
            return _context.Servers.Where(item => item.UseServerExtender).ToList();
        }

        public List<EntityServer> GetAllPublicServers()
        {
            return _context.Servers.Where(item => item.IsPublic).ToList();
        }

        public List<EntityServer> GetServers(EntityUser user)
        {
            if (user == null)
            {
                return _context.Servers.Where(item => item.IsPublic).ToList();
            }
            try
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(user.Id);

                if (usr.IsAdmin)
                {
                    return _context.Servers.ToList();
                }
                return usr.InstanceServerRoles.Select(item => item.Server).Where(item => AuthHelper.HasAccess(item, "SERVER_INFO", 
                    "SERVER_SETTINGS_GLOBAL_RD", 
                    "SERVER_SETTINGS_JOBS_RD", 
                    "SERVER_SETTINGS_BACKUPS_RD",
                    "SERVER_CONFIG_SE_RD", 
                    "SERVER_CONFIG_ME_RD", 
                    "SERVER_EXPLORER_LIST", 
                    "SERVER_MAP_SE_LIST", 
                    "SERVER_MAP_ME_LIST", 
                    "SERVER_PERF_READ",
                    "ACCESS_SERVER_READ",
                    "SERVER_INFO")).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void RemoveServer(EntityServer server)
        {
            try
            {
                _context.PerfEntries.RemoveRange(server.PerfEntries);
                _context.Servers.Remove(server);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
    
}